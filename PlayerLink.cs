using CustomPlayerEffects;
using InventorySystem;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using System.Linq;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

public class PlayerLinkController : MonoBehaviour
{
    private Player _player;
    private Transform _target;
    private ReferenceHub _dummyhub;
    private Player _dummy;
    private bool _lockRotation;
    private float _timer;
    private bool _flashOnEnd;

    private bool _originalMute;
    private Vector3 _originalPos;
    private Quaternion _originalRot;
    private Vector3 _originalScale;
    private bool _wasGodMode;
    private Vector3 _originalGravity;
    private ushort _previousItemSerial;

    private float _startDelay;
    private bool _isStarted;

    private float _endDelay;
    private bool _isEnding;
    private bool _dummyAlive;

    private void OnEnable()
    {
        PlayerEvents.Death += OnPlayerDeath;
        PlayerEvents.Left += OnPlayerLeft;
    }

    private void OnDisable()
    {
        PlayerEvents.Death -= OnPlayerDeath;
        PlayerEvents.Left -= OnPlayerLeft;
    }

    public void Init(Player p, Transform target, bool lockRot, float duration, bool fStart, bool fEnd, bool createDummy)
    {
        _player = p;
        _target = target;
        _lockRotation = lockRot;
        _timer = duration;
        _flashOnEnd = fEnd;

        _originalMute = p.IsMuted;
        _originalPos = p.Position;
        _originalRot = p.Rotation;
        _originalScale = p.Scale;
        _originalGravity = p.Gravity;
        _wasGodMode = p.IsGodModeEnabled;

        _previousItemSerial = p.Inventory.CurItem.SerialNumber;

        if (createDummy)
        {
            SpawnDummy();
        }

        if (fStart)
        {
            p.EnableEffect<Flashed>(1, 1);
            _startDelay = 0.8f;
            _isStarted = false;
        }
        else
        {
            _startDelay = 0f;
            StartMoving();
        }
    }

    private void StartMoving()
    {
        _isStarted = true;
        var hub = _player.ReferenceHub;

        _player.Mute();
        _player.IsGodModeEnabled = true;
        _player.Gravity = Vector3.zero;
        _player.Scale = Vector3.zero;

        if (_target != null)
        {
            _player.Position = _target.position;
            if (_lockRotation) _player.Rotation = _target.rotation;
        }
    }

    void LateUpdate()
    {
        if (_player == null)
        {
            Destroy(this);
            return;
        }

        if (!_isStarted)
        {
            _startDelay -= Time.deltaTime;
            if (_startDelay <= 0) StartMoving();
            return;
        }

        if (_isEnding)
        {
            _endDelay -= Time.deltaTime;
            if (_endDelay <= 0)
            {
                FinalizeLink();
            }
            return;
        }

        if (_target == null || _timer <= 0)
        {
            BeginEndLink();
            return;
        }

        _timer -= Time.deltaTime;

        _player.Position = _target.position;
        if (_lockRotation)
        {
            Vector3 targetEuler = _target.rotation.eulerAngles;
            float pitch = targetEuler.x;
            if (pitch > 180) pitch -= 360;
            pitch = Mathf.Clamp(pitch, -88f, 88f);
            Vector2 finalRotation = new Vector2(pitch, targetEuler.y);
            _player.ReferenceHub.TryOverrideRotation(finalRotation);
        }
        _player.Inventory.ServerSelectItem(0);
    }

    void BeginEndLink()
    {
        if (_flashOnEnd)
        {
            _player.EnableEffect<Flashed>(1, 1);
            _isEnding = true;
            _endDelay = 0.8f;
        }
        else
        {
            FinalizeLink();
        }
    }

    void FinalizeLink()
    {
        if (_player != null)
        {
            var hub = _player.ReferenceHub;
            _player.IsGodModeEnabled = _wasGodMode;
            _player.Gravity = _originalGravity;
            _player.Scale = _originalScale;

            if (_originalMute == false) _player.Unmute(false);

            if (_dummyAlive)
            {
                _player.Position = _originalPos;
                _player.Rotation = _originalRot;

                Timing.CallDelayed(0.1f, () =>
                {
                   if (_previousItemSerial != 0)
                   {
                       Timing.CallDelayed(0.1f, () =>
                       {
                           if (_player != null)
                           {
                               _player.ReferenceHub.inventory.ServerSelectItem(_previousItemSerial);
                           }
                       });
                   }
                });
            }
            else
            {
                _player.ClearInventory();
                _player.SetRole(RoleTypeId.Spectator);
            }
        }
        if (_dummy != null)
        {
            CleanupDummy();
        }

        Destroy(this);
    }

    private void SpawnDummy()
    {
        _dummyhub = DummyUtils.SpawnDummy(_player.Nickname);

            if (_dummyhub == null)
            {
                return;
            }

            Timing.CallDelayed(0.2f, () =>
            {
                _dummy = Player.Get(_dummyhub);

                if (_dummy == null)
                {
                    return;
                }

                _dummy.Role = _player.Role;
                _dummyAlive = true;


                Timing.CallDelayed(0.3f, () =>
                {
                    if (_dummy == null) return;

                    _dummy.IsGodModeEnabled = true;

                    _dummy.Position = _originalPos;
                    _dummy.Rotation = _originalRot;
                    _dummy.Scale = _originalScale;

                    _dummy.CustomInfo = _player.CustomInfo;
                    _dummy.InfoArea = _player.InfoArea;
                    _dummy.UserGroup = _player.UserGroup;
                    _dummy.IsSpectatable = false;

                    foreach (var itemBase in _player.Inventory.UserInventory.Items.Values)
                    {
                        _dummy.AddItem(itemBase.ItemTypeId);
                    }

                    if (_previousItemSerial != 0 && _player.Inventory.UserInventory.Items.ContainsKey(_previousItemSerial))
                    {
                        var originalType = _player.Inventory.UserInventory.Items[_previousItemSerial].ItemTypeId;
                        var matchingItem = _dummy.Items.FirstOrDefault(i => i.Type == originalType);
                        if (matchingItem != null)
                        {
                            _dummy.CurrentItem = matchingItem;
                        }
                    }
                });
            });
    }

    private void CleanupDummy()
    {
        if (_dummy != null)
        {
            NetworkServer.Destroy(_dummy.GameObject);
        }
    }

    private void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
        if (ev.Player != _player) return;

        if (_dummy != null)
        {
            _dummy.Kill();
        }
    }

    private void OnPlayerDeath(PlayerDeathEventArgs args)
    {
        if (args.Player != _dummy) return;

        _dummyAlive = false;
    }
}