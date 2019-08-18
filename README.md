! [тест](https://img.shields.io/badge/test-passing-green.svg)
! [docs](https://img.shields.io/badge/docs-passing-green.svg)
! [платформа](https://img.shields.io/badge/platform-Quartus/Vivado-blue.svg)

USTCRVSoC
===========================
SoC, написанный в SystemVerilog, основанный на RISC-V, Princeton structure

****
## Каталог
* [Особенности] (#особенности)
* [SoC structure] (#SoC structure)
* [Характеристики ЦП] (#характеристики ЦП)
* [Развертывание на FPGA] (#развертывание на FPGA)
    * Развертывание на Nexys4-DDR
    * Развертывание в DE0-Nano
    * Развертывание на других досках разработки
* [Тестовое программное обеспечение] (#тестовое программное обеспечение)
    * Привет Мир
    * Отладка шины с помощью UART
    * Использование экрана VGA
    * Использование инструмента: USTCRVSoC-инструмент
* [Моделирование RTL] (#RTL моделирование)
    * Для моделирования
    * Изменение директивного ПЗУ

# Особенности

* * * CPU**: 5-сегментный конвейер RISC-V, который может работать** RV32I * * большинство инструкций в наборе команд
* * * Шина**: простой и интуитивно понятный, с**механизм рукопожатия**, 32-битный адрес ширина бита и 32-битная ширина бита данных шина
* * * Bus арбитраж**: может быть изменен с помощью определения макросов для облегчения расширения периферийных устройств, DMA, многоядерных и т. д
* * * Интерактивная отладка UART**: поддержка использования Putty на ПК, помощника последовательного порта, minicom и другого программного обеспечения для достижения * * сброс системы**,* * загрузка программы**, * * просмотр памяти * * и т. д.
* * * Чистая реализация RTL**: полностью использует SystemVerilog, не вызывает IP-ядро, легко переносится и эмулируется
* RAM и ROM соответствуют определенному написанию Verilog, * * автоматически интегрируется в BLOCK RAM**

 Структура # SoC

![Image text](https://github.com/Visual-e/USTC-RVSoC/blob/master/images/SoC.png)

На приведенном выше рисунке показана структура SoC, Bus arbitrator * * bus_router * * для Центра SoC, на котором установлены 3 * * основные интерфейсы * * и 5**подчиненные интерфейсы**.Шина, используемая этим SoC, не исходит из какого-либо стандарта (например, AXI или APB Bus), но сама по себе автор, потому что просто называется**naive_bus**.

Каждый * * из интерфейса * * занимает часть адресного пространства.Когда**главный интерфейс**обращается к шине, * * bus_router * * определяет, к какому адресному пространству принадлежит этот адрес, а затем направляет его**к соответствующему**подчиненному интерфейсу**.В следующей таблице показаны 5**адресных пространств из интерфейса**.

/ Тип периферийного устройства / начальный адрес / конечный адрес |
| :-----: | :-----: | :----: |
/ Инструкция ROM / 0x00000000 / 0x00007fff |
/ Инструкция RAM / 0x00008000 / 0x00008fff |
/ ОЗУ данных / 0x00010000 / 0x00010fff |
/ ОЗУ памяти / 0x00020000 / 0x00020fff |
/ Пользователь UART / 0x00030000 / 0x00030003 |

### Составные части

* * * Multi master Multi slave Bus arbitrator**: соответствующие документы naive_bus_router.sv разделите адресное пространство для каждого ведомого устройства и направьте запросы на чтение и запись шины для основного устройства на ведомое устройство. когда несколько первичных устройств одновременно обращаются к одному подчиненному устройству, можно также разрешить конфликты на основе приоритета первичного устройства.
* * * Rv32i Core**: соответствующий файл core_top.sv включает в себя два основных интерфейса:один для получения инструкций и один для чтения и записи данных.
* * * Отладчик UART**: соответствующий файл isp_uart.sv он получает команды от UART и считывает и записывает на шину с главного компьютера. он может использоваться для записи и отладки в интернете. он также может получать команды ЦП для отправки данных пользователю.
* * * Инструкция ROM**: соответствующий файл instr_rom.sv CPU по умолчанию берет инструкции отсюда, и поток инструкций внутри фиксируется, когда аппаратный код компилируется и синтезируется и не может быть изменен во время выполнения. instr_rom.sv * * код, а затем перекомпилировать синтез, записать логику FPGA.Поэтому * * instr_rom * * используется для эмуляции.
* * * Инструкция ОЗУ**: соответствующий файл ram_bus_wrapper.sv пользователь использует isp_uart для записи потока команд в интернете, а затем указывает на адрес загрузки здесь, а затем после сброса SoC CPU запускает поток команд отсюда.
* * * Оперативная память данных**: соответствующий файл ram_bus_wrapper.sv хранение данных во время выполнения.
* * * ОЗУ памяти**: соответствующий файл video_ram.sv на экране отображается 86 столбцов * 32 строки = 2752 символа, 4096b ОЗУ памяти делится на 32 блока, каждый блок соответствует одной строке, составляет 128B, а первые 86 байтов соответствуют 86 столбцам.На экране отображается каждый байт в виде символа, соответствующего коду ASCII.

# Характеристики процессора

* Поддержка:** RV32I * * во всех загрузках, магазинах, арифметике, логике, сдвигах, сравнениях, прыжках.
* Не поддерживается: синхронизация, состояние управления, вызовы среды и инструкции класса точки останова

Все поддерживаемые инструкции включают：

> LB, LH, LW, LBU, LHU, SB, SH, SW, ADD, ADDI, SUB, LUI, AUIPC, XOR, XORI, OR, OR, ANDI, SLL, SLL, SLL, SRL, SRL, SRL, SLT, SLT, SLTU, SLTIU, BEQ ,BNE, BLT, BGE, BLTU, BGEU, JAL, JALR

Что касается набора инструкций, то в будущем вы можете рассмотреть возможность добавления инструкций умножения и деления в **RV32IM**, а затем дополнения к инструкциям, которые не реализованы в**RV32I**

Процессор использует 5-сегментный конвейер,который в настоящее время поддерживает характеристики конвейера：

> Вперед, Loaduse, ожидание рукопожатия шины

Что касается сборочного конвейера, в будущем рассмотрите возможность добавления следующих функций：

> Прогнозирование ветвей, прерывание

# Развертывание на FPGA

В настоящее время мы предлагаем Xilinx **nexys4-DDR** разработка платы и Altera **DE0-Nano** разработка платы.

Для развертывания и тестирования вам необходимо подготовить следующее：

* ПК, оснащенный системой * * Windows7 * * или более поздней версией (если вы используете Linux, трудно использовать несколько инструментов, написанных на C#, которые я предоставил）
* * * Nexys4-DDR * * плата разработки или * * DE0-Nano * * плата разработки или другая плата разработки FPGA
**Среда разработки RTL для платы разработки**, например, Nexys4-DDR для Vivado (рекомендуется для Vivado 2017.4 или более поздней версии), DE0-Nano для Quartus (рекомендуется для Quartus II 11.1 или более поздней версии）
*Если ваша плата разработки не поставляется с ** USB to UART* *схемы (например, DE0-Nano не поставляется), вам нужен **USB to UART модуль**.
* * * Опционный**: * экран, кабель ВГА*

## Развертывание на Nexys4-DDR

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/nexys4-connection2.png)

1. ** Аппаратное соединение**: как показано выше, на плате разработки Nexys4 есть USB-порт, который может использоваться как для записи FPGA, так и для связи UART, нам нужно подключить этот USB-порт к компьютеру.Кроме того, соединение VGA не является обязательным, и вы можете подключить его к экрану.
2. ** Синтез, запись**: пожалуйста, откройте с Vivado**./оборудование / Vivado / nexys4 / USTCRVSoC-nexys4 / USTCRVSoC-nexys4.xpr**.Синтезировать и записывать на доску разработки.


## Развертывание в DE0-Nano

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/DE0-Nano.png)

1, * * аппаратное соединение**: на плате DE0-Nano нет ни последовательного порта USB, ни VGA-интерфейса.Поэтому необходимы внешние модули, а также некоторые практические навыки и знания оборудования.Мы используем два ряда GPIO на DE0-Nano в качестве контактов для внешнего модуля, а интерфейс имеет смысл, как показано выше.Вам нужен модуль USB-to-UART, который соединяет контакты TX и RX UART, чтобы он мог общаться с компьютером.Соединение VGA является необязательным и должно соответствовать определению контактов VGA на рисунке выше.Эффект последнего соединения показан ниже：

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/connection.png)

2, * * синтез, запись**: пожалуйста, откройте с Quartus**./ оборудование / Кварт / DE0_Nano / DE0_Nano.qpf**.Синтезировать и записывать на доску разработки.

## Развертывание на других платах разработки

Если, к сожалению, у вас есть плата разработки FPGA под рукой, которая не является ни Nexys4, ни DE0-Nano, вам нужно будет создать проект вручную, подключив сигнал к верхнему слою платы разработки.Разделены на следующие этапы：

* * * Строительство работ**: после строительства работ необходимо будет**.все sv-файлы в/hardware/ RTL / ** добавляются в проект.
* * * Написание верхнего уровня**: файл верхнего уровня SoC - это**. / hardware / RTL / soc_top.sv**, вам нужно написать файл верхнего уровня для этой платы разработки, вызвать **soc_top* * и подключить контакты FPGA к** soc_top**.Вот описание сигнала для** soc_top**.
* * * Компиляция, синтез, запись в FPGA**

"Verilog
модуль soc_top  #(
  // Коэффициент деления приема UART, пожалуйста, определите тактовую частоту clk, вычислите формулу UART_RX_CLK_DIV=частота clk (Гц) / 460800, округлите
  параметр UART_RX_CLK_DIV = 108,
  // UART передает коэффициент деления, пожалуйста, определите тактовую частоту clk, вычислите формулу UART_TX_CLK_DIV=частота clk (Гц) / 115200, округлите
  параметр UART_TX_CLK_DIV = 434,
  // Коэффициент деления VGA, пожалуйста, определите тактовую частоту clk, вычислите формулу VGA_CLK_DIV=частота clk (Гц) / 50000000
  параметр VGA_CLK_DIV = 1
)(
  вход логический clk, / / SoC часы, рекомендуется использовать кратные 50 МГц
  input logic isp_uart_rx, / / вывод UART RX, подключенный к плате разработки
  output logic isp_uart_tx, / / вывод UART TX, подключенный к плате разработки
  выход logic vga_hsync, vga_vsync, / / подключается к VGA (не может быть подключен）
  output logic vga_red, vga_green, vga_blue / / подключение к VGA (не может быть подключено）
);
` """ 



# Тестирование программного обеспечения

После записи аппаратного обеспечения начните его тестирование

### Проверьте Hello World

После записи аппаратного обеспечения, если на вашей плате разработки есть индикатор UART, вы уже можете видеть, что индикатор TX мигает, и каждый мигающий бит на самом деле отправляет"Hello", что означает, что процессор запускает программу по умолчанию в командном ПЗУ.Ниже мы рассмотрим этот привет.

Во-первых, нам нужно программное обеспечение * * серийного терминала**, например：
* миником
* Помощник последовательного порта
* Супер терминал
* Путти

Ни один из этих инструментов не достаточно прост в использовании, поэтому используйте гаджет, который поставляется с этим хранилищем **UartSession** для демонстрации.Его путь**./ tools / UartSession.exe**.

> ** UartSession * * написано на C#,**.VisualStudio работает в / UartSession-VS2012**.

Во-первых, мы запускаем **UartSession.exe**, можно увидеть, что программное обеспечение перечисляет все доступные порты компьютера и дает несколько вариантов：
* * * Открыть порт**: введите номер, нажмите Enter, чтобы открыть номер соответствующего порта.
* * * Изменение скорости передачи данных**: введите " baud [Number]"и нажмите Enter, чтобы изменить скорость передачи данных.Например, вход baud 9600 может изменить скорость передачи данных 9600.
* * * Обновить список портов**: введите "refresh"и нажмите Enter, чтобы обновить список портов.
* * * Выход**: введите "выход", чтобы выйти

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/UartSession2.png)

Скорость передачи по умолчанию составляет 115200, что соответствует нашему SoC и не требует модификации.Найдите порт, соответствующий плате разработки FPGA, непосредственно из списка портов и откройте его.Мы можем видеть, что окно постоянно показывает "привет", просто не может остановиться,как показано выше, что говорит о том, что процессор работает нормально.

> Если вы не знаете, какой порт в списке портов соответствует плате разработки FPGA, вы можете отключить USB платы разработки и обновить список портов один раз, исчезающий порт-это порт, соответствующий плате разработки.Затем снова подключите USB(если схема в FPGA отсутствует, вам нужно повторно записать FPGA）


### Отладка шины с помощью UART

Теперь интерфейс постоянно печатает "hello", и мы нажимаем Enter,чтобы увидеть, что другой человек больше не нажимает"hello", и появляется"debug", так что он успешно переходит в режим **DEBUG**.

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/UartSession1.png)

Отладчик UART имеет два режима：
* * * Режим пользователя**: в этом режиме пользователь может получать данные, отправленные CPU через isp_uart.FPGA по умолчанию находится в этом режиме после записи.привет, мы можем видеть только в этом режиме.Перейдя в режим отладки, отправив\n **в uart**, вы можете выскочить из **USER mode**.
* * * Режим отладки**: любые данные, напечатанные процессором в этом режиме, будут подавлены, UART больше не будет активно отправлять данные, в виде * * один вопрос один ответ**, команды отладки, отправленные пользователем, и полученные ответы * * заканчиваются на\n**, могут быть возвращены в **режим пользователя**, отправив"o"или сброс системы.

Ниже давайте попробуем функцию отладки * * UART**, введите * * "0"* * и нажмите Enter, вы увидите, что другая сторона отправила 8-разрядную 16-разрядную систему.Это число является данными, считываемыми по адресу 0x00000000 шины SoC, то есть первой инструкцией в * * инструкции ROM**, как показано ниже.

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/UartSession3.png)

В дополнение к чтению мы также можем написать шину с отладчиком, введя команду записи: "10000 abcd1234" и нажав Enter, мы увидим, что другая сторона отправила * *" wr done"**, что означает успешную запись, команда означает запись 0xabcd1234 на адрес 0x10000 (0x10000-это первый адрес ОЗУ данных).

Чтобы убедиться, что запись прошла успешно, введите команду read:**" 10000"**и нажмите Enter, вы увидите, что другая сторона отправила * * "abcd1234"**.

> Примечание: отладчик UART может только выравнивать**4 байта * * за шину чтения и записи и должен читать и писать 4 байта за раз.

В следующей таблице показаны все форматы команд для режима отладки**.

| Тип команды / пример команды / возвращает пример |значение / 
| ----- | :----- | :---- | :----- |
/ Чтение шины / 00020000 / abcd1234 / адрес 0x00020000 считывание данных-0xabcd1234 |
/ Write Bus / 00020004 1276acd0 / wr done / write data to address 0x00020004 0x1276acd0 |
/ Cut to USER mode / o / user / переключение в USER mode
/ Reset / r00008000 / rst done / CPU reset и выполняется с адреса 0x00008000 при переключении обратно в режим пользователя |
/ Незаконные команды / ^^ $ aslfdi / invalid / инструкции для отправки не определены |

> Примечание: все команды, отправленные или полученные, заканчиваются на\N или\r или\r\n,**UartSession.exe * * автоматически вставляется в\n.Если вы используете другое программное обеспечение, такое как помощник последовательного порта, обратите внимание на эту проблему.

Согласно этим командам, нетрудно догадаться, что процесс загрузки программы в интернете：

1. Используйте команду записи, чтобы записать поток команд в ОЗУ команды(адрес ОЗУ команды-00008000~00008fff）
2. Используйте команду сброса r00008000, сбросьте процессор и загрузите его из оперативной памяти команды

### Используйте экран VGA

Вы можете пропустить этот шаг без подключения экрана.

Если плата разработки подключена к экрану через VGA, вы можете увидеть красную рамку на экране, пустую внутри.На самом деле внутри скрыты пробелы символов в 86 Столбцах и 32 строках.Ниже** отладчик UART * * позволяет отображать символы на экране.

> Совет: если красная рамка на экране не находится в центре, вы можете исправить это с помощью кнопки“автокоррекция”на экране

В режиме**DEBUG * * отправьте команду записи： **"20000 31323334"** ，вы можете видеть, что первая строка появляется**4321** и так далее.Это связано с тем, что начальный адрес ОЗУ для видеопамяти равен 0x20000, а отладчик UART записывает 0x34, 0x33, 0x32, 0x31 в первые 4 байта, то есть**4321**код ASCII.

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/vga_show.png)

ОЗУ памяти составляет 4096 байт, разделенных на 32 блока, которые соответствуют 32 строкам на экране; каждый блок 128B, первые 86 байтов соответствуют первым 86-символьным ASCII-кодам в этой строке.Последние 128-86 байт не отображаются на экране.

ОЗУ с видеопамятью ведет себя так же, как и ОЗУ данных, и может быть прочитана и записана, но нет никакой гарантии, что такт будет считывать данные.

### Инструменты использования: USTCRVSoC-инструменты

Долгое время играл с отладкой UART, а также должен был запустить benchmark с процессором.

** .несколько апплетов на языке ассемблера доступны в /software/asm-code** как benchmark,как показано в следующей таблице.

/ Имя файла / описание |
| :----- | :----- |
/ io-test / uart_print.S / пользователь UART циклически печатает hello, т. е.**инструкция ROM**в программе |
/ io-test / vga_hello.S |hello на экране / 
/ расчет-тест / Фибоначчи.S / рекурсивный метод расчета * * серия Фибоначчи * * 8-е число |
/ calculation-test / Number2Ascii.S / преобразует число в строку ASCII, аналогичную * * itoa * * или** sprintf %d * * |
/ calculation-тест / QuickSort.S / инициализирует часть данных в ОЗУ и выполняет** quicksort * * |
/ basic-test / big_endian_little_endian.S / проверьте, является ли эта система**большим концом * * или * * маленьким концом * * (здесь, естественно, небольшой конец) |
/ basic-test / load_store.S / завершает чтение и запись памяти |

** USTCRVSoC-инструмент.exe * * - это гаджет, способный собирать и записывать, что эквивалентно * * IDE языка ассемблера**, путь которого**./инструменты / USTCRVSoC-инструменты.exe**, интерфейс показан ниже.

> * * USTCRVSoC-tool * * написан на C#, инженерный путь VisualStudio ./ USTCRVSoC-tool-VS2012

![Image text] (https://github.com/Visual-e/USTCRVSoC/images/USTCRVSoC-tool-image.png)

Теперь попробуйте заставить SoC запустить программу для быстрой сортировки вычислений.Шаги：
1. ** Откройте USTCRVSoC-инструмент.<url> не удалось найти**
2. ** Открыть**: нажмите кнопку * * Открыть**, чтобы перейти к каталогу ./software / asm-code /calculation-test/, откройте файл сборки * * QuickSort.S**.
3. ** Ассемблер**: нажмите кнопку * * ассемблер**, чтобы увидеть строку шестнадцатеричных чисел в нижней части окна, это машинный код, который получает ассемблер.
4. **Записать запись**: убедитесь, что FPGA подключен к компьютеру и сжег оборудование SoC, затем выберите правильный COM-порт, нажмите * * записать запись**, если в строке состояния ниже указано“записать запись успешно”, процессор уже запустил этот машинный код.
5. ** Просмотр памяти**: в это время, нажав на**DUMP памяти**справа, вы можете увидеть упорядоченную последовательность чисел.Программа QuickSort сортирует беспорядочные массивы -9~ + 9, повторяя каждый номер дважды.Память по умолчанию**DUMP * * не может отображаться полностью,вы можете установить длину до 100, чтобы количество байтов DUMP было 0x100 байт, вы можете увидеть полный результат сортировки.

Кроме того, * * USTCRVSoC-tool * * также может просматривать данные последовательного порта в режиме пользователя.Пожалуйста, откройте * * io-test / uart_print.S**, сборка и запись записи, можно увидеть справа**Serial View * * в поле постоянно печатает привет.

Теперь вы можете попробовать запустить эти сборки benchmark или написать свои собственные сборки для тестирования.** Have fun!**

> О**Princeton structure**: хотя мы различаем**инструкции RAM**,**данные RAM**,**видеопамять RAM**, эта память записи не отличается в Princeton structure.Вы можете записать инструкции в * * Data RAM**,**Video Memory RAM* * для запуска, или вы можете поместить переменные в * * директива RAM**.Даже инструкции и данные могут быть помещены в * * Data RAM**, пока адрес не конфликтует, программа может работать нормально.Но это снижает эффективность работы, потому что**интерфейс команд процессора**и**интерфейс данных**будут**бороться с шиной**.



 Эмуляция # RTL

Склад предлагает** Vivado **и** ModelSim-Altera * * два моделирования среды

### Эмуляция

*Если вы используете ** Vivado**, пожалуйста, откройте проект**./оборудование / Vivado / nexys4 / USTCRVSoC-nexys4 / USTCRVSoC-nexys4.xpr**, проект уже выбран **soc_top_tb.sv * * в качестве верхнего уровня моделирования, вы можете непосредственно**поведение моделирования**.
*Если вы используете ** Quartus**, убедитесь, что у вас также есть компонент** ModelSim-Altera**.Используйте** ModelSim-Altera **открыть**./ оборудование / модели / USTCRVSoC.mpf**, после компиляции, пожалуйста, эмулируйте**soc_top_tb**.

Поток команд, запускаемый при эмуляции, поступает из * * командного ПЗУ**, и если вы еще не изменили**командного ПЗУ**, вы можете увидеть **uart_tx** сигнал при эмуляции **UART** форма волны, отправленная **UART**, это то, что она печатает * * hello**.

> Советы: как правило, при установке **Quartus**, если не намеренно не галочка, автоматически устанавливается на **ModelSim-Altera**

### Изменение директивного ПЗУ

Если вы хотите эмулировать поток команд, вам нужно изменить**командный ROM**.
