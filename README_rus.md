# AutoOverlay AviSynth plugin

### Требования
- AviSynth+ 3.6+: https://github.com/AviSynth/AviSynthPlus/releases/
- AvsFilterNet plugin https://github.com/Asd-g/AvsFilterNet (включено в поставку)
- SIMD Library https://github.com/ermig1979/Simd (включено в поставку)
- Math.NET Numerics (включено в поставку)
- .NET framework 4.6.1+
- Windows 7+

Windows XP и предыдущие версии AviSynth поддерживаются только в версиях плагина ниже 0.2.5.

### Установка
- Скопировать DLL версий x86/x64 в папки с плагинами AviSynth.
- В свойствах DLL в проводнике Windows может потребоваться "Разблокировать" файлы. 

### Описание
Плагин предназначен для оптимального наложения одного видеоклипа на другой. 
Выравнивание клипов относительно друг друга осуществляется фильтром OverlayEngine путем тестирования различных координат верхнего левого угла наложения, размеров изображений, соотношений сторон и углов вращения для того, чтобы найти оптимальные параметры наложения. Функция сравнения двух участков изображения двух клипов - среднеквадратическое отклонение, которое далее обозначается как diff. Задача автовыравнивания - найти минимальное значение diff. 
Для повышения производительности автовыравнивание разделено на несколько шагов масштабирования для тестирования различных параметров наложения, задаваемых фильтром OverlayConfig. На первом шаге тестируются все возможные комбинации наложения в низком разрешении. На каждом следующем шаге в более высоком разрешении тестируются комбинации на базе лучших с предыдущего шага. Конфигурации OverlayConfig могут объединяться в цепочки. Если определенная конфигурация дала хороший результат, тестирование следующих не выполняется для экономии времени. 
После автовыравнивания один клип может быть наложен на другой различными способами с помощью фильтра OverlayRender.

### Подключение плагина
    LoadPlugin("%plugin folder%\AvsFilterNet.dll")
    LoadNetPlugin("%plugin folder %\AutoOverlay_netautoload.dll")
AviSynth+ поддерживает автоподключение плагинов, если имя файла плагина .NET содержит суффикс `_netautoload`, который по умолчанию есть.

## Фильтры
### OverlayConfig
    OverlayConfig(float minOverlayArea, float minSourceArea, float aspectRatio1, float aspectRatio2, 
                  float angle1, float angle2, int minSampleArea, int requiredSampleArea, float maxSampleDiff, 
                  int subpixel, float scaleBase, int branches, float branchMaxDiff, float acceptableDiff, 
                  int correction, int minX, int maxX, int minY, int maxY, int minArea, int maxArea, 
                  bool fixedAspectRatio, bool debug)
     
Фильтр описывает конфигурацию автовыравнивания для OverlayEngine. Он содержит граничные значения параметров наложения таких как: координаты верхнего левого угла накладываемого изображения относительно основного, ширину и высоту накладываемого изображения и угол вращения. Также конфигурация включает параметры работы OverlayEngine. 
Результат работы фильтра - фейковый кадр, в котором закодированы параметры, с помощью чего они могут быть считаны в OverlayEngine. 
Существует возможность объединять несколько конфигурация в цепочки с помошью обычного объединения клипов: OverlayConfig(…) + OverlayConfig(…). В этом случае OverlayEngine будет тестировать каждую конфигуарцию последовательно на каждом шаге пока не будет получен приемлемое (acceptable) значение diff. 

#### Параметры
- **minOverlayArea** - минимальное отношение используемой части к общей площади накладываемого изображения в процентах. По умолчанию рассчитывается таким образом, чтобы накладываемый клип мог полностью перекрыть базовый (режим пансканирования). К примеру, если разрешение основного клипа 1920x1080, а накладываемого 1920x800, то значение параметра будет 800/1080=74%. 
- **minSourceArea** - минимальное отношение используемой части к общей площади основного изображения в процентах. По умолчанию рассчитывается таким образом, чтобы основной клип мог полностью включить накладываемый (режим пансканирования). К примеру, если разрешение основного клипа 1920x1080, а накладываемого 1440x800, то значение параметра будет 1440/1920=75%. 
- **aspectRatio1** and **aspectRatio2** - диапазон допустимых соотношений сторон накладываемого изображения. По умолчанию - соотношение сторон накладываемого клипа. Может быть задан в любом порядке: `aspectRatio1=2.35, aspectRatio2=2.45` то же самое, что и `aspectRatio1=2.45, aspectRatio2=2.35`.
- **angle1** и **angle2** (default 0) - диапазон допустимых углов вращения накладываемого изображения. Может быть задан в любом порядке. Отрицательные значения – вращение по часовой стрелке, положительные – против.
- **minSampleArea** (default 1500) – минимальная площадь в пикселях базового изображения на первом шаге. Чем меньше, тем быстрее, но выше риск некорректного результата. Рекомендованный диапазон: 500-3000. 
- **requiredSampleArea** (default 3000) - максимальная площадь в пикселях базового изображения на первом шаге. Чем меньше, тем быстрее, но выше риск некорректного результата. Рекомендованный диапазон: 1000-5000.
- **maxSampleDiff** (default 5) – максимально допустимое значение diff уменьшенного базового изображения между шагами. Если превышает указанное значение, то предыдущий шаг выполнен не будет. Используется для выбора начального размера изображения между minSampleArea и requiredSampleArea и соответственно шага.
- **subpixel** (default 0) – величина наложения с субпиксельной точностью. 0 – точность один пиксель, 1 – половина пикселя, 2 – четверть и т.д. Ноль рекомендуется, если один клип имеет существенно более низкое разрешение, чем другой. 1-3 рекомендуется, если оба клипа имеют примерно одинаковое разрешение. Отрицательные значения тоже поддерживаются, в этом случае наложение будет выполнено с пониженной точностью, но быстрее. 
- **scaleBase** (default 1.5) – основание для расчета уменьшающего коэффициента по формуле `coef=scaleBase^(1 - (maxStep - currentStep))`. Чем ниже, тем большее количество шагов. 
- **branches** (default 1) - какое количество наилучших параметров наложения использовать с предыдущего шага для поиска на текущем. Больше - лучше, но дольше. По сути, глубина ветвления.    
- **branchMaxDiff** (default 0.2) - максимальная разница на текущем шаге между значениями diff наилучших параметров поиска и прочих. Используется для отбрасывания бесперспективных ветвей поиска.  
- **acceptableDiff** (default 5) – приемлемое значение diff, после которого не тестируются последующие конфигурации в цепочке OverlayConfig.
- **correction** (default 1) – величина коррекции некоторого показателя на текущем шаге с предыдущего. Чем выше, тем больше различных параметров тестируется, но это занимает больше времени. 
- **minX**, **maxX**, **minY**, **maxY** - допустимые диапазоны координат левого верхнего угла наложения, по умолчанию не ограничены. 
- **minArea**, **maxArea** - диапазон допустимой площади накладываемого изображения, в пикселях. По умолчанию не ограничен.
- **fixedAspectRatio** (default false) - режим точного соотношения сторон накладываемого клипа, только для случая, когда aspectRatio1=aspectRatio2.
- **debug** (default false) – отображение параметров конфигурации, медленно.

### OverlayEngine                  
    OverlayEngine(clip source, clip overlay, string statFile, int backwardFrames, int forwardFrames, 
                  clip sourceMask, clip overlayMask, float maxDiff, float maxDiffIncrease, float maxDeviation, 
                  int panScanDistance, float panScanScale, bool stabilize, clip configs, string presize, 
                  string resize, string rotate, bool editor, string mode, float colorAdjust, bool simd, bool debug)

Фильтр принимает на вход два клипа: основной и накладываемый и выполняет процедуру автовыравнивания с помощью изменения размера, вращения и сдвига накладываемого клипа, чтобы найти наименьшее значение diff. Оптимальные параметры наложения кодируются в выходной кадр, чтобы они могли быть считаны другими фильтрами. Последовательность таких параметров наложения кадра за кадром (статистика) может накапливаться в оперативной памяти, либо в файле для повторного использования без необходимости повторно выполнять дорогостоящую процедуру автовыравнивания. Файл статистики может быть проанализирован и отредактирован во встроенном графическом редакторе. 

#### Параметры
- **source** (required) - первый, основной клип.
- **overlay** (required) - второй, накладываемый клип. Оба клипа должны быть в одном и том же типе цветового пространства (YUV или RGB) и глубине цвета. Поддерживаются планарные YUV (8-16 бит), RGB24 и RGB48 цветовые пространства.
- **statFile** (default empty) – путь к файлу со статистикой параметров наложения. Если не задан, то статистика накапливается только в оперативной памяти в пределах одного сеанса. Рекомендуемый сценарий использования: для начального подбора параметров вручную не использовать файл статистики, а использовать для тестового прогона, чтобы собрать статистику, проанализировать и подправить в редакторе. 
- **backwardFrames** and **forwardFrames** (default 3) – количество анализируемых предыдущих и последующих кадров в одной сцене для стабилизации и ускорения поиска параметров наложения. 
- **sourceMask**, **overlayMask** (default empty) – маски для основного и накладываемого клипа. Если маска задана, то пиксели клипа, которым соответствует значение 0 в маске, игнорируются при расчете DIFF. Подходит, к примеру, для исключения логотипа из расчета diff. В RGB клипах каналы анализируются раздельно. В YUV анализируется только канал яркости. Маска должна быть в полном диапазоне (`ColorYUV(levels="TV->PC")`).
- **maxDiff** (default 5) – diff ниже этого значения интерпретируются как успешные. Используется для детектирования сцен. 
- **maxDiffIncrease** (default 1) – максимально допустимое превышение diff текущего кадра от среднего значения в последовательности (сцене).
- **maxDeviation** (default 1) – максимально допустимая разница в процентах между объединением и пересечением двух конфигураций выравнивания для обнаружения сцен. Более высокие значения могут привести к ошибочному объединению нескольких сцен в одну, но обеспечивают лучшую стабилизацию в пределах сцены. 
- **panScanDistance** (default 0) – максимально допустимый сдвиг накладываемого изображения между соседними кадрами в сцене. Используется, если источники не стабилизированы относительно друг друга.
- **panScanScale** (default 3) – максимально допустимое изменения размера в промилле накладываемого изображения между соседними кадрами в сцене.
- **stabilize** (default true) – попытка стабилизировать кадры в самом начале сцены, когда еще не накоплено достаточное количество предыдущих кадров. Если true, то параметр `panScanDistance` должен быть 0.
- **configs** (по умолчанию OverlayConfig со значениями по умолчанию) – список конфигураций в виде клипа. Пример: `configs=OverlayConfig(subpixel=1, acceptableDiff=10) + OverlayConfig(angle1=-1, angle2=1)`. Если в ходе автовыравнивания после прогона первой конфигурации будет получено значение diff менее 10, то следующая конфигурация с более "тяжелыми" параметрами (вращение) будет пропущена. 
- **presize** (default *BilinearResize*) – функция изменения размера изображения для начальных шагов масштабирования.
- **resize** (default *BicubicResize*) – функция изменения размера изображения для финальных шагов масштабирования.
- **rotate** (default *BilinearRotate*) – функция вращения изображения. В настоящее время по умолчанию используется реализация из библиотеки AForge.NET.
- **editor** (default false). Если true, во время загрузки скрипта запустится визуальный редактор. 
- **mode** (default "default") – режим работы со статистикой:  
DEFAULT – по умолчанию
UPDATE – как предыдущий, но DIFF текущего кадра всегда перерассчитывается
ERASE – стереть статистику (используется для очистки информации об определенных кадрах совместно с функцией Trim)
READONLY – использовать, но не пополнять файл статистики
PROCESSED – включить только уже обработанные кадры
- **colorAdjust** - not implemented yet
- **simd** (default true) - использование SIMD Library для повышения производительности в некоторых случаях
- **debug** (default false) - отображение параметров наложения, снижает производительность

#### Принцип работы
*OverlayEngine* ищет оптимальные параметры наложения: координаты верхнего левого угла накладываемого клипа относительного основного, угол вращения в градусах, ширина и высота накладываемого изображения, а также величины обрезки накладываемого изображения по краям для субпиксельного позиционирования.  
Цепочка *OverlayConfig* в виде клипа описывает границы допустимых значений и алгоритм поиска оптимальных. Движок прогоняет каждую конфигурацию друг за другом пока не будут найдены параметры наложения с приемлемым diff. Процесс автовыравнивания для каждой конфигурации содержит несколько шагов. На первом шаге тестируются все возможные комбинации параметров наложения в низком разрешении. На следующий шаг передается некоторое количество наилучших комбинаций, задаваемых параметром `OverlayConfig.branches`. На каждом следующем шаге параметры наложения конкретизируются в более высоком разрешении, область поиска задается параметром *correction*.
Масштабирование изображений выполняется функциями, заданными в параметрах *presize* and *resize*. Первый используется на предварительных шагах автовыравнивания, второй на финальных, когда работа ведется в полном разрешении. На финальных шагах рекомендуется использовать фильтр с хорошей интерполяцией. Функция масштабирования должно иметь следующую сигнарутуру: `Resize(clip clip, int target_width, int target_height, float src_left, float src_top, float src_width, float src_height)`. Допускаются дополнительные параметры. Такая же сигнатура используется в стандартных функциях AviSynth. Крайне рекомендуется использовать плагин ResampleMT, который дает тот же результат, что и встроенные фильтры, но работает значительно быстрее за счет параллельных вычислений.  
В ходе автовыравнивания движок может анализировать соседние кадры с помощью параметров *backwardFrames* and *forwardFrames* parameters according to *maxDiff, maxDiffIncrease, maxDeviation, stabilize, panScanDistance, panScanScale* по следующему алгоритму:  
1. Требуется предоставить параметры наложения текущего кадра.
2. Если в статистике уже есть данные по кадру, возврат из кэша. 
3. Если предыдущие кадры в количестве *backwardFrames* уже обработаны и их параметры наложения одинаковы, а значения diff не превышают *maxDiff*, то будут протестированы такие же параметры наложения и для текущего кадра.
4. Если полученный diff не превышает *maxDiff* и среднее значение сцены более чем на *maxDiffIncrease*, то запускается анализ последующих кадров в количестве *forwardFrames*, иначе кадр будет отмечен как начало новой сцены.
5. Последующие кадры тестируются так же, как и текущий. Если все они подходят, то текущий кадр точно останется в сцене. 
6. Если один из последующих кадров не подойдет, будет запущен процесс автовыравнивания для этого кадра. Если полученные оптимальные параметры наложения не сильно отличаются от текущих, текущий кадр не будет включен в сцену, т.к. возможно тоже слегка смещен относительно предыдущего. Этот процесс регулируется параметром *maxDeviation*.
7. Если текущий кадр отмечен как независимый и включен параметр *stablize=true*, будет произведена попытка выровнять первые *backwardFrames* кадров одинаковым образом, чтобы начать новую сцену. 
8. Если параметр *backwardFrames* равен нулю, каждый кадр обрабатывается индивидуально. Это занимает больше времени и может вызвать дрожание картинки.
9. Если источники не стабилизированы относительно друг друга, необходимо использовать *panScanDistance* и *panScanScale*. 

##### Визуальный редактор
Запускается, если *OverlayEngine.editor*=true.  
Слева превью кадра. Внизу трекбар по количеству кадров и поле ввода текущего кадра. Справа таблица, отображающая кадры с одинаковыми параметрами наложения, объединенные в эпизоды. Между эпизодами можно переключаться. Под гридом панель управления.  
Overlay settings - параметры наложения текущего эпизода. 

Ниже секция *AutoOverlay*.  
Кнопка Single frame - повторный прогон AutoOverlay для текущего кадра, характеристики наложения распространяются на весь эпизод. Кнопка separated frame применяет результат только к текущему кадру. 
Кнопка Scene - повторный прогон AutoOverlay для всех кадров, составляющих эпизод, в результате чего он может быть разбит на несколько.  
Справа секция для обработки "пансканированных" сцен. 


Измененные и несохраненные эпизоды подсвечиваются желтым цветом в гриде. Кнопка save - сохранение изменений. Reset - сброс изменений и повторная загрузка данных. Reload - перезагрузка характеристик для текущего кадра, распространяющиеся на весь эпизод.  
Separate - обособление кадра. Join prev - присоединить кадры предыдущего эпизода. Join next - присоединить кадры следующего эпизода. Join to - присоединить кадры до введенного включительно.  

**Hotkeys**:
* Ctrl + S - save
* Ctrl + R - reload
* D - enable/disable difference
* P - enable/disable preview
* Ctrl + arrow keys - move overlay image
* Ctrl + add/subtract - scale overlay image
* A, Z - next/previous frame

### OverlayRender
    OverlayRender(clip engine, clip source, clip overlay, clip sourceMask, clip overlayMask, string overlayMode, 
                  int width, int height, string pixelType, int gradient, int noise, bool dynamicNoise, 
                  clip borderOffset, clip srcColorBorderOffset, clip overColorBorderOffset, int mode, float opacity, 
                  float colorAdjust, string adjustChannels, string matrix, string upsize, string downsize, string rotate, 
                  bool simd, bool debug, bool invert, bool extrapolation, int blankColor, float background, int backBlur)
                  
Фильтр осуществляет рендеринг результата совмещения двух клипов с определенными настройками.

#### Параметры
- **engine** (required) - клип типа *OverlayEngine*, который предоставляет параметры наложения.
- **source** (required) - первый, основной клип.
- **overlay** (required) - второй клип, накладываемый на первый. Поддерживаются планарные YUV (8-16 бит), RGB24 и RGB48 цветовые пространства.
- **sourceMask** and **overlayMask** (default empty) - маски основного и накладываемого клипа. В отличие от OverlayEngine смысл этих масок такой же, как в обычном фильтре *Overlay*. Маски регулируют интенсивность наложения клипов относительно друга друга.
- **overlayMode** (default blend) – режим наложения для встроенного фильтра `Overlay`
- **width** и **height** - ширина и высота выходного изображения. По умолчанию соответствует основному клипу.
- **pixelType** - не реализован. Вместо него для результирующего клипа используется цветовое пространство основного клипа. 
- **gradient** (default 0) - длина прозрачного градиента в пикселях по краям накладываемой области. Делает переход между изображениями более плавным.
- **noise** (default 0) - длина градиента шума в пикселях по краям накладываемой области. Делает переход между изображениями более плавным.
- **dynamicNoise** (default true) - динамический шум по краям изображения от кадра к кадру, если *noise* > 0.
- **borderOffset** (default empty) - клип типа *Rect* для задания "пустых" границ изображения (left, top, right, bottom), которые будут проигнорированы при расчете градиентной маски.
- **srcColorBorderOffset** (default empty) - клип типа *Rect* для определения "пустых" границ основного клипа (left, top, right, bottom), которые будут проигнорированы при цветокоррекции.
- **overColorBorderOffset** (default empty) - клип типа *Rect* для определения "пустых" границ накладываемого клипа (left, top, right, bottom), которые будут проигнорированы при цветокоррекции.
- **mode** (default 1) – overlay and cropping mode:  
1 - обрезка по краям основного изображения.  
2 - совмещение обоих изображений с обрезкой по краям выходного клипа.  
3 - совмещение обоих изображений без обрезки.  
4 - как 3, только с заполнением пустых углов по типу ambilight.  
5 - как 3, только с заполнением всего пустого пространства по типу ambilight.  
6 - маска режима 3. Используется для совмещения результата с еще одним клипом.   
- **opacity** (default 1) - степень непрозрачности накладываемого изображения от 0 до 1.
- **colorAdjust** (default -1, disabled) - вещественное значение между 0 и 1. 0 - стремление к цвету основного клипа. 1 - накладываемого клипа. 0.5 - усредненный цвет. Цветокоррекция основана на сравнении гистограмм области пересечения. 
- **adjustChannels** (default empty) - в каких каналах регулировать цвет. Примеры: "yuv", "y", "rgb", "rg".
- **matrix** (default empty). Если параметр задан, YUV изображение конвертируется в RGB по указанной матрице для цветокоррекции.
- **downsize** и **upsize** (default *BicubicResize*) - функции для уменьшения и увеличения размера изображений.
- **rotate** (default *BilinearRotate*) - функция вращения накладываемого изображения.
- **simd** (default *true*) – использование SIMD Library для повышения производительности в некоторых случаях
- **debug** - вывод параметров наложения.
- **invert** - поменять местами основной и накладываемый клипы, "инвертировать" параметры наложения. 
- **extrapolation** - то же самое, что ColorAdjust.extrapolation.
- **blankColor** (default black) - цвет в HEX формате `0xFF8080` для заполнения пустот в режимах 3 и 4.
- **background** (default 0) - вещественное значение между -1 и 1 для задания источника заблюренного фона в режимах 2,4,5. -1 - основной клип, 1 - накладываемый клип.
- **backBlur** (default 15) - сила смазывания в режимах 2,4,5.

### ColorAdjust
    ColorAdjust(clip sample, clip reference, clip sampleMask, clip referenceMask, float intensity, 
	            bool limitedRange, string channels, float dither, float exclude, string interpolation, 
				bool extrapolation, bool dynamicNoise, bool simd, bool debug)

Автокоррекция цвета. Входной клип, sample и reference клипы должны быть в одном типе цветового диапазона (YUV or RGB). Поддерживаются любые планарные цветовое диапазоны YUV (8-16 bit), RGB24 и RGB48. Входной клип и sample клип должны иметь одинаковую глубину цвета (обычно sample - это весь входной фильтр или его часть). Глубина цвета входного фильтра изменится на глубину цвета клипа reference. Фильтр дает хороший результат только если sample и reference клипы содержат схожее наполнение кадра. Фильтр используется внутри OverlayRender, но может использоваться и независимо. 

#### Параметры
- **clip** (required) - входной клип, цвет которого будет отрегулирован
- **sample** (required) - the sample clip (usually the first clip or cropped) 
- **reference** (required) - reference clip (usually the same time and area from different master)
- **sampleMask** and **referenceMask** (default empty) - 8 битные планарные маски для включения в обработку только участков изображений, значение маски для которых равно 255.
- **intensity** (default 1) - интенсивность цветокоррекции
- **limitedRange** (default true) - ТВ диапазон
- **channels** (default yuv or rgb) - плоскости или каналы для обработки. Допустимы любые комбинации y,u,v или r,g,b (пример: y, uv, r, br).
- **dither** (default 0.95) - уровень дизеринга 0 (disable) to 1 (aggressive). 
- **exclude** (default 0) - исключение редко встречающихся в изображениях цветов по формуле: *current_color_pixel_count / total_pixel_count < exclude*.
- **interpolation** (default spline) - алгоритм интерполяции из библиотеки Math.NET Numerics (spline, akima, linear).
- **extrapolation** (default false, experimental) - экстраполяция цветов, выходящих за границы сэмплов.
- **dynamicNoise** (default true) - динамический шум, если цветовая карта совпадает у нескольких кадров.
- **simd** (default true) - использование SIMD Library для повышения производительности в некоторых случаях

### ComplexityOverlay
    ComplexityOverlay(clip source, clip overlay, string channels, int steps, float preference, bool mask, float smooth, bool debug)
    
Независимый фильтр для совмещения наиболее сложных участков двух клипов. Подходит для совмещения двух источников низкого качества. Клипы должны иметь одинаковые кадрирование, цвет, разрешения и цветовые пространства. 

#### Parameters
- **source** and **overlay** - входные клипы
- **channels** (default yuv or rgb) - плоскости или каналы для обработки. Допустимы любые комбинации y,u,v или r,g,b (пример: y, uv, r, br).
- **steps** (default 1) - количество шагов формирования маски совмещения. 
- **preference** (default 0) - если больше ноля 0 второй клип будет более предпочтителен, иначе первый клип. Рекомендуется: -1 to 1. 
- **mask** (default false) - выводить маску наложения вместо совмещения. 
- **smooth** (default 0) - смазать маску наложения для снижения резкости.

### OverlayCompare
    OverlayCompare(clip engine, clip source, clip overlay, string sourceText, string overlayText, int sourceColor, 
                   int overlayColor, int borderSize, float opacity, int width, int height, bool debug)
This filter generates comparison clip from source and overlay clips with borders and labels.

#### Parameters
- **engine** (required) - *OverlayEngine* clip.
- **source** (required) - first, base clip.
- **overlay** (required) - second, overlaid clip.
- **sourceText** (default "Source") - source clip name.
- **overlayText** (default "Source") - overlay clip name.
- **sourceColor** (default 0x0000FF) - source clip border color.
- **overlayColor** (default 0x00FF00) - overlay clip border color.
- **borderSize** (default 2) - border size.
- **opacity** (default 0.51) - opacity of overlay clip.
- **width** (source clip width by default) - output width.
- **height** (source clip height by default) - output height.
- **debug** (default false) - print align settings. 

### StaticOverlayRender
    StaticOverlayRender(clip source, clip overlay, int x, int y, float angle, int overlayWidth, int overlayHeight, 
                        float cropLeft, float cropTop, float cropRight, float cropBottom, float diff, 
                        clip sourceMask, clip overlayMask, string overlayMode, int width, int height, 
                        string pixelType, int gradient, int noise, bool dynamicNoise, clip borderOffset, 
                        clip srcColorBorderOffset, clip overColorBorderOffset, int mode, float opacity,
                        float colorAdjust, string adjustChannels, string matrix, string upsize, string downsize, 
                        string rotate, bool simd, bool debug, bool invert, bool extrapolation, int blankColor, float background, int backBlur)

As OverlayRender but with fixed align settings without OverlayEngine.

#### Parameters
- **source** (required) - first, base clip.
- **overlay** (required) - second, overlaid clip.
- **x** (required) - x coordinate.
- **y** (required) - y coordinate.
- **angle** (default 0) - rotation angle.
- **overlayWidth** (overlay clip width by default) - width of overlay clip after resize.
- **overlayHeight** (overlay clip height by default) - height of overlay clip after resize.
- **cropLeft**, **cropTop**, **cropRight**, **cropBottom** (default 0) - crop overlay clip before resize for subpixel alignment.
- **diff** (default 0) - DIFF value for debug output. 
Other parameters are same as for *OverlayRender* filter.

### CustomOverlayRender
    CustomOverlayRender(clip engine, clip source, clip overlay, string function, int width, int height, bool debug)
This filter allows to override default overlay algorithms by user using overlay settings from OverlayEngine. 

#### Parameters
- **engine** (required) - *OverlayEngine* clip.
- **source** (required) - first, base clip.
- **overlay** (required) - second, overlaid clip.
- **function** (required) - user function name. The function must have the following parameters: `clip engine, clip source, clip overlay, int x, int y, float angle, int overlayWidth, int overlayHeight, float cropLeft, float cropTop, float cropRight, float cropBottom, float diff)`
- **width** (source clip width by default) - output clip width.
- **height** (source clip height by default) - output clip height.
- **debug** (default false) - debug mode.

## Rect
    Rect(int left, int top, int right, int bottom, bool debug)
    
Support filter to use as argument on other clips. It representes a rectangle. 
    
#### Parameters
Left, top, right, bottom integer values. 

### ColorRangeMask
    ColorRangeMask(clip, int low, int high)
Support filter which provides mask clip by color range: white if pixel value is between low and high arguments. For YUV clips only luma channel is used. For RGB clips all channels are proccessed independently. Output is the clip in the same color space. Limited range is not supported. 

#### Parameters
- **input** (required) - input clip.
- **low** (default 0) - lower bound of color range.
- **high** (default 0) - higher bound of color range.

### BilinearRotate
    BilinearRotate(clip, float)
Support filter for rotation by angle with bilinear interpolation.

#### Parameters
- **input** (required) - input clip.
- **angle** (required) - rotation angle.

### OverlayMask
    OverlayMask(clip template, int width, int height, 
                int left, int top, int right, int bottom, 
                bool noise, bool gradient, int seed)
Support filter which provides mask clip for overlay with gradient or noise at borders.

#### Parameters
- **template** (default empty) - if specified width, height and color space will be used from template clip for output.
- **width** - output clip width if template is not specified. 
- **height** - output clip height if template is not specified.
- **left**, **top**, **right**, **bottom** - border size.
- **noise** - noise generation on borders.
- **gradient** - gradient borders.
- **seed** - seed for noise generation.


## Examples
#### Simple script 
    OM=AviSource("c:\test\OpenMatte.avs") # YV12 clip
    WS=AviSource("c:\test\Widescreen.avs") # YV12 clip
    OverlayEngine(OM, WS, configs = OverlayConfig(subpixel=2)) 
    OverlayRender(OM, WS, debug = true)
#### Analysis pass without render. Aspect ratio range was specified. Set editor=true after analysis pass.
    OM=AviSource("c:\test\OpenMatte.avs") # YV12 clip
    WS=AviSource("c:\test\Widescreen.avs") # YV12 clip
    config=OverlayConfig(aspectRatio1=2.3, aspectRatio2=2.5)
    OverlayEngine(OM, WS, configs = config, statFile="c:\test\Overlay.stat", editor=false)
#### Render after analysis pass
    OM=AviSource("c:\test\OpenMatte.avs") # YV12 clip
    WS=AviSource("c:\test\Widescreen.avs") # YV12 clip
    OverlayEngine(OM, WS, statFile="c:\test\Overlay.stat")
    OverlayRender(OM, WS, debug=true, noise=50, upsize="Spline64Resize")
    ConvertToYV12()

## История изменений
### 29.08.2020 v0.3.1
1. Исправлена ошибка кодирования x264 первого кадра.
2. ColorAdjust: исправлена HDR экстраполяция, параметр dynamicNoise.
3. OverlayRender: параметр extrapolation.

### 28.08.2020 v0.3
1. OverlayEngine: presize и resize вместо upsize и downsize.
2. OverlayEngine: новый режим PROCESSED.
3. OverlayEngine: поддержка пансканирования (нестабилизированных источников). 
4. Использование SIMD Library для повышения производительности. 
5. ColorAdjust: новые алгоритмы интерполяции с помощью Math.NET Numerics.
6. AviSynth API v8 
7. OverlayEditor: new features. 
8. OverlayRender: new features.
9. ComplexityOverlay: new filter.
