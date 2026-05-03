using UnityEngine;
using LabApi.Features.Wrappers;
using CustomPlayerEffects;
using Logger = LabApi.Features.Console.Logger;

public class PlayerLinkController : MonoBehaviour
{
    private Player _player;
    private Transform _target;
    private bool _lockRotation;
    private float _timer;
    private Transform _returnPoint;
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

    public void Init(Player p, Transform target, bool lockRot, float duration, bool fStart, bool fEnd)
    {
        Logger.Debug($"Initializing PlayerLinkController for player {p.Nickname} (ID: {p.UserId}) with target {(target != null ? target.name : "null")}, lockRot: {lockRot}, duration: {duration}, flashOnStart: {fStart}, flashOnEnd: {fEnd}");
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

        if (fStart)
        {
            p.ReferenceHub.playerEffectsController.EnableEffect<Flashed>(1f);
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
        var hub = _player.GameObject.GetComponent<ReferenceHub>();

        _player.Mute();
        _player.Scale = Vector3.zero;
        _player.IsGodModeEnabled = true;
        _player.Gravity = Vector3.zero;
        _previousItemSerial = hub.inventory.CurItem.SerialNumber;


        if (_target != null)
        {
            _player.Position = _target.position;
            if (_lockRotation) _player.Rotation = _target.rotation;
        }
        hub.inventory.ServerSelectItem(0);
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
            float yaw = targetEuler.y;
            _player.Rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        if (_player.ReferenceHub.inventory.CurItem.SerialNumber != 0)
        {
            _player.ReferenceHub.inventory.ServerSelectItem(0);
        }
    }

    void BeginEndLink()
    {
        if (_flashOnEnd)
        {
            _player.ReferenceHub.playerEffectsController.EnableEffect<Flashed>(1f);

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
            var hub = _player.GameObject.GetComponent<ReferenceHub>();
            _player.IsGodModeEnabled = _wasGodMode;
            _player.Gravity = _originalGravity;
            if (_originalMute == false) _player.Unmute(false);

            if (_previousItemSerial != 0)
            {
                hub.inventory.ServerSelectItem(_previousItemSerial);
            }

            if (_returnPoint != null)
            {
                _player.Position = _returnPoint.position;
                _player.Rotation = _returnPoint.rotation;
                _player.Scale = _originalScale;
            }
            else
            {
                _player.Position = _originalPos;
                _player.Rotation = _originalRot;
                _player.Scale = _originalScale;
            }
        }

        Destroy(this);
    }
}