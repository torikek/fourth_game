# Описание проекта

Прототип трёхмерной игры с видом от первого лица на Godot с использованием C#. Игрок исследует квартиру, взаимодействует с предметами, читает книги и собирает записки. Двери можно открывать и закрывать, а стулья и дверные полотна — разрушать ударами.

Взаимодействие построено на интерфейсах: каждый предмет определяет доступные ему действия. При наведении на предмет появляется подсказка. Для книг и записок используется общее окно чтения, а количество собранных записок отображается на экране. Удары и разрушение сопровождаются объёмным звуком, при разрушении появляются осколки.

## Структура

**Сцены находятся в папке `scenes/`:**

1. **scene.tscn** — основная сцена формата `Node3D`.
   Содержит игрока, два экземпляра модели квартиры, три двери, три стула, две книги, три записки, шкафы и тумбу. У одной двери включены автоматическое закрытие после прохода и последующее запирание.

2. **player.tscn** — управляемый игроком объект формата `CharacterBody3D`.
   Содержит модель, коллизию, камеру `Camera3D`, луч `RayCast3D` для выбора предметов и интерфейс с подсказкой взаимодействия и счётчиком записок. Перемещается относительно направления взгляда и учитывает гравитацию.

3. **Door.tscn** — дверь формата `Node3D`.
   Содержит раму, поворотный узел `DoorHinge` с дверным полотном, `AnimationPlayer` и область прохода `PassageArea`. Открытие и закрытие используют одну анимацию `Open`, воспроизводимую в прямом или обратном направлении. При разрушении полотно удаляется вместе с коллизиями, а рама остаётся.

4. **Chair.tscn** — разрушаемый стул формата `StaticBody3D`.
   Содержит сиденье, спинку, ножки и коллизии. По умолчанию имеет 10 единиц здоровья и разрушается от одного удара игрока.

5. **Book.tscn** — книга формата `StaticBody3D`.
   Содержит импортированную модель и коллизию. При взаимодействии открывает собственный текст, заданный в инспекторе. После чтения остаётся на месте и доступна повторно.

6. **Note.tscn** — записка формата `Area3D`.
   Содержит модель и область обнаружения лучом. Сначала показывает текст, а после закрытия окна добавляется в инвентарь и исчезает из мира. Уникальность предмета определяется полем `ItemId`.

7. **ReadingPanel.tscn** — окно чтения формата `CanvasLayer`.
   Содержит заголовок, прокручиваемый текст `RichTextLabel` и кнопку закрытия. Создаётся игроком и используется для всех книг и записок.

8. **Shkaf.tscn**, **Tumba.tscn** — предметы обстановки формата `Node3D`.
   Содержат геометрию шкафов и тумбы с текстурами. Отдельные скрипты взаимодействия к ним не подключены.

**Модели, текстуры, звуки и вспомогательная сцена мебели `Furniture.tscn` находятся в папке `resources/`.**

## Скрипты

**Основные скрипты находятся в папке `scripts/`:**

1. **Player.cs** — обрабатывает перемещение, обзор мышью, взаимодействие, атаку и инвентарь.
   Метод `UpdateInteractionTarget()` проверяет луч камеры и поднимается от коллайдера к родительским узлам в поисках `IInteractable`. Метод `ShowText()` открывает окно чтения, освобождает курсор и блокирует движение, обзор и действия с предметами. Метод `CloseReading()` возвращает управление и уведомляет предмет о завершении чтения. Метод `CollectItem()` сохраняет уникальный идентификатор предмета, обновляет счётчик и вызывает подбор.

2. **ReadingPanel.cs** — управляет общим окном чтения.
   Метод `ShowText()` подставляет текст, прокручивает его к началу и переводит фокус на кнопку закрытия. Нажатие кнопки отправляет событие `CloseRequested`, которое обрабатывает игрок.

3. **ApartmentImport.gd** — скрипт обработки импортируемой сцены квартиры на GDScript.
   Для вогнутых коллизий дочерних узлов стен `Wall*` включает `backface_collision`, чтобы тонкие стены останавливали игрока с обеих сторон.

**Скрипты предметов находятся в папке `scripts/objects/`:**

1. **Door.cs** — управляет здоровьем, анимацией и состоянием двери.
   Метод `Interact()` переключает открытие и закрытие доступной двери. При включённом `AutoCloseAfterPassage` скрипт отслеживает вход и выход игрока с разных сторон проёма, затем закрывает дверь после освобождения прохода и задержки. Параметр `LockAfterAutoClose` запирает дверь после завершения анимации. Метод `ReceiveDamage()` уменьшает здоровье, а `Die()` создаёт эффект разрушения и удаляет полотно. По умолчанию дверь имеет 50 единиц здоровья, задержка закрытия — 0,4 секунды.

2. **Chair.cs** — хранит здоровье стула и обрабатывает урон.
   При исчерпании здоровья воспроизводит звук разрушения, создаёт осколки и удаляет стул. Метод `Interact()` не выполняет действия: для стула предусмотрена атака.

3. **Book.cs** — открывает текст книги через игрока из группы `reading_host`.
   После закрытия окна книга остаётся в мире и может быть прочитана снова.

4. **Note.cs** — связывает чтение записки с её подбором.
   Метод `OpenText()` открывает текст, `CloseText()` передаёт записку игроку для добавления в инвентарь, а `Collect()` скрывает и удаляет предмет. Проверки состояния защищают от повторного чтения и подбора во время обработки текущего действия.

5. **ObjectSound.cs** — воспроизводит объёмные звуки удара и разрушения из `resources/audio/`.
   Создаёт отдельный `AudioStreamPlayer3D` в основной сцене, поэтому удаление предмета не прерывает звук. После окончания воспроизведения источник удаляется.

6. **DestructionEffect.cs** — создаёт разлёт 32 осколков на месте разрушенного предмета.
   Использует материалы исходной геометрии, задаёт случайные скорости и вращение, имитирует падение и уменьшает осколки перед исчезновением. Эффект автоматически удаляется через 0,85 секунды.

## Интерфейсы

**Интерфейсы находятся в папке `scripts/interface/`:**

1. **IInteractable.cs** — общий контракт взаимодействия с методом `Interact()`. Используется игроком для работы с разными предметами через одну систему выбора цели.

2. **IDamageable.cs** — расширяет `IInteractable` методами `ReceiveDamage()` и `Die()`. Реализуется дверями и стульями.

3. **IReadable.cs** — расширяет `IInteractable` методами `OpenText()` и `CloseText()`. Реализуется книгами и записками.

4. **ICollectible.cs** — расширяет `IInteractable` свойством `ItemId` и методом `Collect()`. Используется для добавления записок в инвентарь без повторения идентификаторов.

5. **IReadingHost.cs** — связывает читаемые предметы с игроком. Метод `ShowText()` запрашивает показ текста, а `CollectItem()` передаёт предмет для подбора. Реализуется классом `Player`.

## Управление

| Клавиша или действие | Назначение |
| --- | --- |
| W, A, S, D | Перемещение игрока |
| Движение мыши | Поворот игрока и обзор камерой |
| E при наведении на предмет | Открытие или закрытие двери, чтение книги или записки |
| Левая кнопка мыши | Один удар по двери или стулу |
| E или Esc во время чтения | Закрытие текста и возврат управления |
| Кнопка «Закрыть (E / Esc)» | Закрытие окна чтения |
| Esc вне окна чтения | Выход из игры |

Для взаимодействия подойдите к предмету и наведите на него камеру до появления подсказки. Во время чтения курсор становится видимым, перемещение и обзор отключаются. Записка подбирается только после закрытия её текста; книга остаётся на месте.

## Ход игры

Игрок начинает исследование квартиры и может свободно перемещаться между комнатами, читать книги, искать записки и взаимодействовать с дверями. В текущей сцене размещены три записки. После чтения и закрытия каждой записки её идентификатор добавляется в инвентарь, а счётчик `Notes` увеличивается.

Одна из дверей автоматически закрывается и запирается после прохода игрока на другую сторону. Запертую дверь нельзя открыть клавишей E, но можно разрушить ударами. При стандартном уроне игрока в 10 единиц для разрушения двери с полным здоровьем требуется пять нажатий левой кнопки мыши. Стул разрушается от одного удара.

В текущем прототипе нет проверки победы или поражения: сбор всех записок не завершает игру. Инвентарь хранится в памяти игрока и не сохраняется между запусками.
UML
<mxfile host="app.diagrams.net">
  <diagram id="project-uml" name="Классы, интерфейсы и сцены">
    <mxGraphModel dx="1069" dy="3608" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="3000" pageHeight="2900" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        <mxCell id="IInteractable" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#d5e8d4;strokeColor=#82b366;fontSize=15;" value="&amp;#171;interface&amp;#187;&lt;br&gt;&lt;b&gt;IInteractable&lt;/b&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+void Interact()&lt;/div&gt;" vertex="1">
          <mxGeometry height="145" width="150" x="550" y="-2850" as="geometry" />
        </mxCell>
        <mxCell id="IReadable" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#d5e8d4;strokeColor=#82b366;fontSize=15;" value="&amp;#171;interface&amp;#187;&lt;br&gt;&lt;b&gt;IReadable&lt;/b&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+void OpenText()&lt;br&gt;+void CloseText()&lt;/div&gt;" vertex="1">
          <mxGeometry height="145" width="170" x="830" y="-2020" as="geometry" />
        </mxCell>
        <mxCell id="ICollectible" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#d5e8d4;strokeColor=#82b366;fontSize=15;" value="&amp;#171;interface&amp;#187;&lt;br&gt;&lt;b&gt;ICollectible&lt;/b&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+ItemId : string&lt;hr&gt;+void Collect()&lt;/div&gt;" vertex="1">
          <mxGeometry height="146" width="150" x="1030" y="-2690" as="geometry" />
        </mxCell>
        <mxCell id="IDamageable" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#d5e8d4;strokeColor=#82b366;fontSize=15;" value="&amp;#171;interface&amp;#187;&lt;br&gt;&lt;b&gt;IDamageable&lt;/b&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+void ReceiveDamage(float damage)&lt;br&gt;+void Die()&lt;/div&gt;" vertex="1">
          <mxGeometry height="145" width="240" x="980" y="-2490" as="geometry" />
        </mxCell>
        <mxCell id="IReadingHost" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#d5e8d4;strokeColor=#82b366;fontSize=15;" value="&amp;#171;interface&amp;#187;&lt;br&gt;&lt;b&gt;IReadingHost&lt;/b&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+bool ShowText(IReadable source, string text)&lt;br&gt;+void CollectItem(ICollectible item)&lt;/div&gt;" vertex="1">
          <mxGeometry height="145" width="370" x="2360" y="-2170" as="geometry" />
        </mxCell>
        <mxCell id="Player" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;class &amp;#183; C#&amp;#187;&lt;br&gt;&lt;b&gt;Player&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: CharacterBody3D&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;-gravity : float&lt;br&gt;-Speed : float&lt;br&gt;-MouseSensitivity : float&lt;br&gt;-Inventory : Array&amp;lt;string&amp;gt;&lt;br&gt;-AttackDamage : float&lt;br&gt;-_camera : Camera3D&lt;br&gt;-_rayCast : RayCast3D&lt;br&gt;-_interactionLabel : Label&lt;br&gt;-_targetNode : Node&lt;br&gt;-_target : IInteractable&lt;br&gt;-_readingPanel : ReadingPanel&lt;br&gt;-_notesLabel : Label&lt;br&gt;-_readingSource : IReadable&lt;br&gt;-_isReading : bool&lt;hr&gt;+void _Ready()&lt;br&gt;+void _PhysicsProcess(double delta)&lt;br&gt;+void _Process(double delta)&lt;br&gt;-void UpdateInteractionTarget()&lt;br&gt;+void _Input(InputEvent event)&lt;br&gt;+bool ShowText(IReadable source, string text)&lt;br&gt;-void CloseReading()&lt;br&gt;+void CollectItem(ICollectible item)&lt;/div&gt;" vertex="1">
          <mxGeometry height="506" width="310" x="270" y="-2560" as="geometry" />
        </mxCell>
        <mxCell id="ReadingPanel" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;class &amp;#183; C#&amp;#187;&lt;br&gt;&lt;b&gt;ReadingPanel&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: CanvasLayer&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+CloseRequested : Action&lt;hr&gt;+void _Ready()&lt;br&gt;+void ShowText(string text)&lt;/div&gt;" vertex="1">
          <mxGeometry height="164" width="210" x="1460" y="-1830" as="geometry" />
        </mxCell>
        <mxCell id="Book" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;class &amp;#183; C#&amp;#187;&lt;br&gt;&lt;b&gt;Book&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: StaticBody3D&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+Text : string&lt;hr&gt;+void Interact()&lt;br&gt;+void OpenText()&lt;br&gt;+void CloseText()&lt;/div&gt;" vertex="1">
          <mxGeometry height="182" width="170" x="1620" y="-2050" as="geometry" />
        </mxCell>
        <mxCell id="Note" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;class &amp;#183; C#&amp;#187;&lt;br&gt;&lt;b&gt;Note&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: Area3D&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+ItemId : string&lt;br&gt;+Text : string&lt;br&gt;-_host : IReadingHost&lt;br&gt;-_reading : bool&lt;br&gt;-_collected : bool&lt;hr&gt;+void Interact()&lt;br&gt;+void OpenText()&lt;br&gt;+void CloseText()&lt;br&gt;+void Collect()&lt;/div&gt;" vertex="1">
          <mxGeometry height="250" width="190" x="1490" y="-2640" as="geometry" />
        </mxCell>
        <mxCell id="Chair" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;class &amp;#183; C#&amp;#187;&lt;br&gt;&lt;b&gt;Chair&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: StaticBody3D&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+MaxHealth : float&lt;br&gt;-_health : float&lt;hr&gt;+void _Ready()&lt;br&gt;+void Interact()&lt;br&gt;+void ReceiveDamage(float damage)&lt;br&gt;+void Die()&lt;/div&gt;" vertex="1">
          <mxGeometry height="218" width="220" x="1800" y="-2420" as="geometry" />
        </mxCell>
        <mxCell id="Door" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;class &amp;#183; C#&amp;#187;&lt;br&gt;&lt;b&gt;Door&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: Node3D&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+MaxHealth : float&lt;br&gt;+AutoCloseAfterPassage : bool&lt;br&gt;+AutoCloseDelay : float&lt;br&gt;+LockAfterAutoClose : bool&lt;br&gt;+IsLocked : bool&lt;br&gt;-_lockOnAnimationFinished : bool&lt;br&gt;-_passageArea : Area3D&lt;br&gt;-_entrySides : Dictionary&amp;lt;Player,float&amp;gt;&lt;br&gt;-_closePending : bool&lt;br&gt;-_closeCountdown : double&lt;br&gt;-_health : float&lt;br&gt;-_isOpen : bool&lt;br&gt;-_destroyed : bool&lt;br&gt;-_animationPlayer : AnimationPlayer&lt;hr&gt;+void _Ready()&lt;br&gt;+void Interact()&lt;br&gt;-void OnPassageEntered(Node3D body)&lt;br&gt;-void OnPassageExited(Node3D body)&lt;br&gt;+void _PhysicsProcess(double delta)&lt;br&gt;-void OnAnimationFinished(StringName animation)&lt;br&gt;+void ReceiveDamage(float damage)&lt;br&gt;+void Die()&lt;/div&gt;" vertex="1">
          <mxGeometry height="452" width="360" x="550" y="-1730" as="geometry" />
        </mxCell>
        <mxCell id="ObjectSound" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;static class&amp;#187;&lt;br&gt;&lt;b&gt;ObjectSound&lt;/b&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;{static} +void Hit(Node3D source)&lt;br&gt;{static} +void Break(Node3D source)&lt;br&gt;{static} -void Play(Node3D source, string path)&lt;/div&gt;" vertex="1">
          <mxGeometry height="149" width="340" x="2120" y="-1895" as="geometry" />
        </mxCell>
        <mxCell id="DestructionEffect" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;class &amp;#183; C#&amp;#187;&lt;br&gt;&lt;b&gt;DestructionEffect&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: Node3D&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;-_shards : List&amp;lt;ShardTuple&amp;gt;&lt;br&gt;-_age : float&lt;br&gt;-Lifetime : float&lt;hr&gt;{static} +void Spawn(Node3D source)&lt;br&gt;{static} -void CollectMeshes(Node node, List&amp;lt;MeshInstance3D&amp;gt; meshes)&lt;br&gt;+void _Process(double delta)&lt;/div&gt;" vertex="1">
          <mxGeometry height="218" width="490" x="1720" y="-1570" as="geometry" />
        </mxCell>
        <mxCell id="ApartmentImport" parent="1" style="rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;spacing=12;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=15;" value="&amp;#171;tool script &amp;#183; GDScript&amp;#187;&lt;br&gt;&lt;b&gt;ApartmentImport&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#555555&quot;&gt;Godot: EditorScenePostImport&lt;/font&gt;&lt;hr&gt;&lt;div style=&quot;text-align:left;font-family:monospace;font-size:12px&quot;&gt;+Object _post_import(Node scene)&lt;/div&gt;" vertex="1">
          <mxGeometry height="145" width="240" x="1130" y="-1830" as="geometry" />
        </mxCell>
        <mxCell id="e1" edge="1" parent="1" source="IReadable" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;strokeColor=#49637d;" target="IInteractable" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e2" edge="1" parent="1" source="ICollectible" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;strokeColor=#49637d;" target="IInteractable" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e3" edge="1" parent="1" source="IDamageable" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;strokeColor=#49637d;" target="IInteractable" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e4" edge="1" parent="1" source="Player" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;dashed=1;strokeColor=#49637d;" target="IReadingHost" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e5" edge="1" parent="1" source="Book" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;dashed=1;strokeColor=#49637d;" target="IReadable" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e6" edge="1" parent="1" source="Note" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;dashed=1;strokeColor=#49637d;" target="IReadable" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e7" edge="1" parent="1" source="Note" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;dashed=1;strokeColor=#49637d;" target="ICollectible" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e8" edge="1" parent="1" source="Door" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;dashed=1;strokeColor=#49637d;" target="IDamageable" value="">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e9" edge="1" parent="1" source="Chair" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=block;endFill=0;dashed=1;strokeColor=#49637d;" target="IDamageable" value="">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="1510" y="-2311" />
              <mxPoint x="1510" y="-2360" />
            </Array>
          </mxGeometry>
        </mxCell>
        <mxCell id="e11" edge="1" parent="1" source="Player" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="IInteractable" value="_target [0..1]">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e12" edge="1" parent="1" source="Player" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="IReadable" value="_readingSource [0..1]">
          <mxGeometry relative="1" x="0.6463" y="-5" as="geometry">
            <mxPoint as="offset" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e13" edge="1" parent="1" source="Player" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="ICollectible" value="CollectItem">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="870" y="-2307" />
              <mxPoint x="870" y="-2617" />
            </Array>
          </mxGeometry>
        </mxCell>
        <mxCell id="e14" edge="1" parent="1" source="Player" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;entryX=0;entryY=0.75;entryDx=0;entryDy=0;" target="IDamageable" value="ReceiveDamage">
          <mxGeometry relative="1" x="0.0755" as="geometry" />
        </mxCell>
        <mxCell id="e15" edge="1" parent="1" source="Player" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="Door" value="подсказка IsLocked">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e16" edge="1" parent="1" source="Door" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="Player" value="_entrySides [0..*]">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e17" edge="1" parent="1" source="Book" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="IReadingHost" value="группа reading_host">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e18" edge="1" parent="1" source="Note" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="IReadingHost" value="_host [0..1]">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e19" edge="1" parent="1" source="Door" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="ObjectSound" value="Hit / Break">
          <mxGeometry relative="1" x="-0.2232" as="geometry">
            <mxPoint as="offset" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e20" edge="1" parent="1" source="Door" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="DestructionEffect" value="Spawn">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e21" edge="1" parent="1" source="Chair" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="ObjectSound" value="Hit / Break">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e22" edge="1" parent="1" source="Chair" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=12;labelBackgroundColor=#ffffff;endArrow=open;dashed=1;strokeColor=#777777;" target="DestructionEffect" value="Spawn">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
