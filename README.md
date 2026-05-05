### English Version 
## Installation
The latest plugin build can be found in the Releases section. Unity scripts are available via Code -> Download ZIP.

* Place ZAMERT.dll into your plugins folder (default: %appdata%\SCP Secret Laboratory\LabAPI\plugins\global).
* Add the contents of the ZZAMERT (Tools) folder to your Unity project (under SL-CustomObjects).

## Features
In addition to the original ZAMERT functions, this version includes the PlayerLink module. It binds a player's position (and optionally rotation) to a specific object ("camera"). Camera movement is handled by animations from other modules.
## Module Settings
<img width="454" height="298" alt="image" src="https://github.com/user-attachments/assets/14d7925e-09a5-47b1-8679-86118d9cdd4c" />

* Target Object Name — The name of the camera object. Must exactly match the primitive name in the schematic.
* Lock Rotation — Whether to sync the player's view rotation with the camera's rotation.
* Duration — Total duration of the effect (in seconds).
* Flash On Start / End — Applies a flashbang effect at the start or end of the sequence.
* Target Type — Defines which players will be affected:
* All: All living players.
   - Zone: Players within areas specified in Target Zone.
   - Around: Players within the radius specified in Range.
* Target Zone — Selected zones for the Zone target type.
* Range — Distance for the Around target type.

## Important Notes

* WARNING: Avoid changing X and Z rotation in camera animations to prevent visual glitches for players.
* It is recommended to make the camera primitive invisible (uncheck Visible in the primitive settings).
* During the link, players are granted immortality, muted, scaled to zero, and have their physics/inventory disabled. Once the duration ends, the player is teleported back and all stats are restored.
* If Flash On Start is enabled, the teleport occurs after a 0.8s delay. Sync your camera animations accordingly.
* If using a collider to trigger this module, be aware that players returning to their original position might re-trigger the collider immediately.

---------------

### Russian Version 
## Установка
Последнюю версию плагина можно найти в разделе Releases. Скрипты для Unity доступны по кнопке Code -> Download ZIP.

* Поместите ZAMERT.dll в папку с плагинами (по умолчанию: %appdata%\SCP Secret Laboratory\LabAPI\plugins\global).
* Добавьте содержимое папки ZZAMERT (Tools) в ваш проект Unity (в папку SL-CustomObjects).

## Функционал
Помимо оригинальных функций, добавлен модуль PlayerLink. Он привязывает позицию (и опционально поворот) игрока к выбранному объекту («камере»). Движение камеры настраивается через анимации в других модулях.
## Настройки модуля
<img width="454" height="298" alt="image" src="https://github.com/user-attachments/assets/14d7925e-09a5-47b1-8679-86118d9cdd4c" />

* Target Object Name — Имя объекта-камеры. Должно точно совпадать с названием примитива в схематике.
* Lock Rotation — Привязывать ли вращение взгляда игрока к повороту камеры.
* Duration — Длительность эффекта в секундах.
* Flash On Start / End — Ослеплять ли игрока в начале или в конце действия.
* Target Type — Фильтр игроков для привязки:
* All: Все живые игроки.
   - Zone: Игроки в зонах, указанных в Target Zone.
   - Around: Игроки в радиусе, указанном в Range.
* Target Zone — Список зон для типа Zone.
* Range — Радиус для типа Around.

## Важные примечания

* ВНИМАНИЕ: Не изменяйте поворот камеры по осям X и Z в анимациях во избежание некорректного отображения у игроков.
* Рекомендуется делать объект-камеру невидимым (снять галочку Visible в настройках примитива).
* Во время привязки игроки становятся бессмертными, невидимыми (размер 0), получают мут, отключают физику и не могут использовать инвентарь. По завершении все параметры и позиция игрока возвращаются к исходному состоянию.
* Если Flash On Start включен, перенос игрока происходит с задержкой 0.8 сек. Учитывайте это при настройке таймингов анимации.
* Если активация происходит через триггер-коллайдер, помните: при возврате в исходную точку игрок может снова коснуться коллайдера и запустить логику повторно.
