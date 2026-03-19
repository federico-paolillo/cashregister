# Zjiang ZJ-8370 — 80mm Thermal Receipt Printer Setup Manual

## 1. Product Information

The Zjiang ZJ-8370 is an 80mm thermal receipt printer designed for POS (Point of Sale) applications. It is also commonly sold under generic "80mm Thermal Receipt Printer" branding by various resellers.

### 1.1 Technical Parameters and Features

| Parameter               | Value                                                                                                  |
|--------------------------|-------------------------------------------------------------------------------------------------------|
| Printer Method           | Line Thermal                                                                                          |
| Print Speed              | 220–300 mm/s                                                                                          |
| Fonts                    | Simplified Chinese 24x24 point (GB18030), Taiwan and HK traditional (complex font), Korean, Japanese  |
| Character Size (Font A)  | 12x24 dot, 1.5(W) x 3.0(H) mm — 576 dot/line or 512 dot/line ANK Character                          |
| Character Size (Font B)  | 9x17 dot, 1.1(W) x 2.1(H) mm                                                                        |
| Character Size (Font C)  | 9x24 dot, 1.1(W) x 3.0(H) mm                                                                        |
| Character Size (CJK)     | Simplified/Traditional: 24x24 dot, 3.0(W) x 3.0(H) mm                                               |
| Extended Character Table | OEM437, Katakana, OEM850, OEM860, OEM863, OEM865, WestEurope, Greek, Hebrew, EastEurope, Iran, WPC1252, OEM866, OEM852, OEM858, IranII, Latvian, Arabic, PT151, 1251, OEM747, WPC1257, Vietnam, OEM864, Hebrew, WPC1255, Thai |
| Logo / Trademark         | Download and printing support                                                                         |
| Interface                | Serial / USB / Ethernet / Bluetooth / WiFi (option)                                                   |
| Print Command            | ESC/POS Command                                                                                       |
| Roll Width               | 79.5 +/- 0.5 mm (Print Width 72 mm)                                                                  |
| Roll Diameter            | 80 mm                                                                                                 |
| Paper Thickness           | 0.06–0.08 mm                                                                                          |
| Power Supply             | DC 24V / 2.5A                                                                                         |
| Auto Cutter              | Full / Partial Cut                                                                                    |
| 1D Barcode Types         | UPC-A, UPC-E, JAN13 (EAN13), JAN8 (EAN8), CODE39, ITF, CODABAR, CODE93, CODE128                     |
| 2D Barcode               | QR CODE                                                                                               |
| Operating Temperature    | 0–45 C                                                                                                |
| Contrast Humidity        | 10–80%                                                                                                |
| Supported Drivers        | Win2000, Win2003, WinXP, Win Vista, Win7, Win8, Win10, Linux                                          |

### 1.2 Main Features

- High-quality printing, high-speed and strong stability
- Interface: Serial RS232 / RJ45 Ethernet / USB / Bluetooth / WiFi
- Android device can be supported if connected with USB / Bluetooth / WiFi methods
- iOS device can be supported if connected with RJ45 Ethernet / Bluetooth / WiFi method

## 2. Communication Ports

### 2.1 Universal Serial Bus (USB Port)

Connect the printer with the standard USB cable (USB device types are automatically detected). Once the printer is connected with the PC, install the driver, then choose the corresponding port accordingly.

### 2.2 RJ45 Ethernet Port (10M/100M)

With this port, you can use the network cable and connect the computer directly for communication. The default port number is **9100**.

### 2.3 RS232 Serial Port

RS-232 is developed according to the EIA standard asynchronous transmission. The specification is as follows:

| Parameter            | Value                                              |
|----------------------|----------------------------------------------------|
| Data transmission    | Serial interface                                   |
| Synchronization      | Asynchronous                                       |
| Signal Level         | +5.4V Serial RS232 Level; logic 1: -5.4V, logic 0: +5.4V |
| Hardware Flow Control| Hardware                                           |
| Baud Rate            | 9600 bps to 115200 bps (Optional)                  |
| Data Length           | 8 Bits                                             |
| Stop Bit             | 1 Bit                                              |
| Parity               | None                                               |

The wiring method of the serial interface printer follows the standard rules of Serial Interface EIA standard. The consumer can get the current default baud rate from the Self-Test Page.

## 3. Basic Function Operations

### 3.1 Indicator Light and Printer Status

The printer has 3 LED indicators: **Power**, **Error**, and **Paper Out**.

- The Power indicator will light on once power is plugged in.
- The Error LED will light on when any error arises (Paper Out / Temperature is too high / mal-position of the carriage unit on the print head).
- Press the **FEED** button to test the paper feeding function if the power indicator looks well.

### 3.2 Printer Operation

- **Switch on:** Ensure the adapter is properly connected with the printer, press the switch on the "1" position; the printer turns on.
- **Switch off:** Press the switch on the other side "O".
- **Paper Feeding:** Press the "FEED" button when power is on; paper will keep running. Stop feeding when you release this button.
- **Self-Test Page:** Switch off the printer, then press the "FEED" button and do not release. Press the "POWER" button at the same time till the "PAPER OUT" indicator comes on, release the button and you can then read the current settings from the test page.
- **Hexadecimal printing:** Power off the printer, press "FEED" button and do not release, press "POWER" button at the same time until the "PAPER OUT" indicator comes on, release the "FEED" button about 3 seconds after the "PAPER OUT" indicator turns off. The printer will then run into Hexadecimal printing mode. To quit Hexadecimal printing mode, restart the printer.

## 4. Clean the Print Head

### When to Clean

You need to clean the print head if problems appear such as:

1. Printing is not clear, but thermal paper is OK
2. Some columns on the printed pages are not clear
3. The paper feeding is noisy

### Cleaning Steps

1. Power off, open the printer cover and take away the paper roll.
2. Wait a while if the printer is just finished printing.
3. Use a soft brush or wrung-out ethanol-immersed cotton to clean the print head. Remember to turn on the gear while lubricating to ensure a completely lubricated result.

### Maintenance Notices

- Make sure the printer is powered off during maintenance.
- Keep hands or other metal tools away from the surface of the printer head; do not use tweezers to scratch the surface of the printer head and other sensitive parts.
- Do not use gasoline, acetone and other organic solvents for printer head cleaning.

## 5. Communication Methods

Ensure all cables are connected correctly (e.g., USB cable / Ethernet cable / Serial cable). Open the cover and load the paper rolls (thermal side facing up).

### 5.1 USB Port Connection

1. Open "Properties" then choose "Ports". Select the unoccupied USB port and click "Apply".
2. Click "General" and then "Print Test Page". If the test page works well, all settings are OK.
3. If "Print Error" appears, go back to "Ports" and try another USB port until it works.

### 5.2 Ethernet Port Connection

1. Get the IP address from the Self-Test Page. (Switch off the printer, press "FEED" button and do not release, press the "POWER" button at the same time till the "ERROR" indicator comes on, then release the button. The self-test page with network parameters will be printed.)
2. The **Default IP Address** of the printer is: **192.168.1.100**
3. Open Printer Driver "Properties" and click "Ports", select "Add Port".
4. Use "Add Standard TCP/IP Printer Port Wizard".
5. Enter the printer IP address (e.g., 192.168.1.100).
6. Choose "Generic Network Card" then click Next.
7. Click Finish and verify the port is added.
8. Go back to "General" and click "Print Test Page" to verify.

### 5.3 Serial Port Connection

1. Check the Printer Driver Properties "Ports" and select the appropriate COM port (e.g., COM1).
2. Select the COM port and click "Configure Port". Make sure the settings match the self-test page values:
   - Bits per second: **115200** (default, check self-test page)
   - Data bits: **8**
   - Parity: **None**
   - Stop bits: **1**
   - Flow control: **Hardware**
3. Click "Apply".
4. Go back to "General" and click "Print Test Page" to verify.

### 5.4 Device Settings

You can also change settings through "Device Settings" in the printer driver Properties (print method setting, cash drawer setting, paper cutter setting, etc.).

If the print speed is too slow, try: "Device Settings" -> "Print Mode" -> select "Print as soft font".

## 6. DHCP Instructions

DHCP stands for "Dynamic Host Configuration Protocol".

### Obtain the Network Address

**Operation:** In the power-on state, open the paper warehouse cover, press the FEED button (press about 1 second), and then close the paper warehouse cover.

**Effect:** The printer will print out the current network parameters.

**Note:** Make sure the machine is connected to the network normally during operation, and the printer has paper. After obtaining the IP, the printer defaults to static IP.

### DHCP OFF

**Operation:** In the power-off state, open the paper warehouse cover, press the FEED button and turn on the printer till the light flashes alternately. Release the FEED button and then press the FEED button **4 times** in a row, and then close the paper warehouse cover.

**Effect:** The printer will turn OFF the DHCP function.

**Note:** Make sure the printer has paper when closing the paper warehouse cover.

### DHCP ON

**Operation:** In the power-off state, open the paper warehouse cover, press the FEED button and turn on the printer till the light flashes alternately. Release the FEED button and then press the FEED button **3 times** in a row, and then close the paper warehouse cover.

**Effect:** The printer will turn ON the DHCP function.

**Note:** Make sure the printer has paper when closing the paper warehouse cover.

## 7. WiFi IP Address Setting

The setting parameters of the 80mm WiFi thermal printer can be modified through the web page. Steps:

1. Power on the printer.
2. Find "HF-LPB100" wireless network and link to it.
3. Enter the IP address **10.10.100.254** into the PC browser.
4. Login credentials:
   - **User name:** admin
   - **Password:** admin
5. Select "AP+STA mode" and save.
6. Go to "STA Setting" and click "Scan" to find your wireless network.
7. Select your wireless network, input the correct configuration:
   - Router name (SSID)
   - WiFi password
   - Static IP settings (IP settings should be in the same network segment as the router, and the IP cannot be occupied by other devices)
8. Click "Save" when STA setting is complete.
9. Select "Other Setting" and configure:
   - Serial Port Parameters: Default baud rate **115200**, Data Bit: 8, Parity Bit: None, Stop Bit: 1
   - Network Parameters: Protocol **TCP/IP**, Port: **9100**
10. Click "Save".
11. Restart the module by going to "Restart" and clicking OK.
12. Verify the connection by pinging the printer's IP address from the computer's command prompt (e.g., `ping 192.168.1.105 -t`). Successful replies mean the printer is connected to the wireless network.

## 8. General Troubleshooting

| # | Problem | Solution |
|---|---------|----------|
| 1 | Serial port printer printing messy code | Ensure the baud rate setting in the PC is the same as the printer baud rate (check baud rate from self-test page). |
| 2 | Self-test page is OK, but printer does not work after driver installation | Recheck the installation steps and verify that the correct port is selected. |
| 3 | Cash drawer does not open | Enable the cash drawer function from device settings of the printer driver. |
| 4 | Malfunction of paper feeding | Use a soft brush to clean the paper sensor and try again, or send it to the service center for repair. |
| 5 | Messy code printing after a period of time | Interface board may be damaged; please replace or repair it. |
| 6 | Malfunction of the indicator light | Check the cable connecting way / adapter / power cord / switch on or off. Or contact the service center for repair. |
| 7 | Ethernet printer does not print after linked with a router | Make sure the network cable and router port work well. Check that the printer IP and router IP are on the same network segment. Check if the printer IP is occupied by another device. Modify the IP with the printer tools attached in the CD disc, or contact the service center. |
| 8 | Messy code printing except Arabic numeral printing | Use the printer tools to change the language to "ASCII". |
| 9 | Malfunction of the auto paper cutting | Check the printer tools: Function Set -> Cutter Set, and make sure the option is ON. Or contact the service center. |
| 10 | Red indicator keeps flashing during printing | Usually caused by high temperature of the print head. Close the printer for a while and let it cool down. Or contact the service center. |
| 11 | Communication interrupts with many printers on Ethernet at the same time | Check the network to get each printer's Ethernet ID. Ensure there are no IP conflicts. Give a specified Ethernet ID to each printer. |
| 12 | Printing speed is very slow for serial printer | Change settings: Driver -> Properties -> Device Settings -> Print Mode -> Print as soft font. |
| 13 | Water or other liquid spills into the printer | Cut off the power. Dry the main board or printer head with a hair dryer at about 50 degrees C. |
| 14 | Print paper without any content (Blank) | 1. If normal print is OK but self-test page is blank (no content), the font chips may need replacement. 2. If no content for both normal print and self-test, replace the head and try again. Or contact the service center. |
| 15 | Reset the printer to factory default setting | Use the printer tools to reset: "Printer tools" -> "Factory Reset". |

## 9. Safety Precautions

### Warnings

- **Scratch Warning:** Never try to touch the auto cutter or jagged teeth.
- **Scald Warning:** To avoid being burned, please do not touch the printer head.
- **Shock Warning:** Please cut off the power before you plug/unplug the power cable from the printer.

### Cautions

1. Please apply the power adapter to a stable voltage (110–220V). Please do not use other devices on the same power outlet to avoid voltage fluctuation.
2. Install the printer on a flat and stable surface to avoid printer suffering from any vibration and shocks.
3. If water or other liquid spills into the printer, unplug the power cord immediately and contact your dealer or service center for advice.
4. Disconnect the power cord if the printer is idle for a long time.
5. Please ensure the switch button is "off" before you plug the power cord.
6. The printer should only be disassembled or repaired by a technician.
7. Please strictly follow the recommended use tips of this manual.

## 10. Additional Developer and Troubleshooting Notes

This section contains supplementary information commonly useful when developing for or troubleshooting the Zjiang ZJ-8370 and similar 80mm ESC/POS thermal printers.

### 10.1 Common Network Defaults

| Setting          | Default Value      |
|------------------|--------------------|
| IP Address       | 192.168.1.100      |
| Subnet Mask      | 255.255.255.0      |
| Gateway          | 192.168.1.1        |
| DHCP             | Disabled           |
| TCP Port         | 9100               |
| MAC Address      | Shown on self-test page |

### 10.2 USB VID/PID Notes

Many Zjiang-based 80mm printers appear with USB Vendor ID **0x0416** or **0x4B43** (varies by OEM). When writing udev rules on Linux or identifying the device programmatically, use `lsusb` output or the self-test page to confirm. A typical udev rule for Linux might be:

```
SUBSYSTEM=="usb", ATTR{idVendor}=="0416", MODE="0666"
```

### 10.3 Thermal Paper Recommendations

- Use paper that conforms to the 79.5 +/- 0.5 mm width and 0.06–0.08 mm thickness specification.
- Low-quality thermal paper accelerates print head wear and can cause faded output. Use paper rated for at least 5 years of image retention for receipt archiving.
- Store thermal paper away from heat, direct sunlight, and solvents (including plasticizers in PVC wallets).

### 10.4 Print Head Lifespan

The thermal print head is rated for approximately **100 km** of printing (or ~150 million pulses). Actual lifespan depends on print density settings, paper quality, and environmental conditions.

### 10.5 Ethernet Connection Tips

- If you cannot ping the printer, verify the PC and printer are on the same subnet.
- Some routers block inter-device communication by default (AP isolation). Disable AP isolation if the printer is on WiFi.
- When connecting directly via Ethernet (no router), use a static IP on the PC in the same subnet (e.g., PC: 192.168.1.50, Printer: 192.168.1.100).
- The printer listens on TCP port 9100. You can test connectivity by sending raw ESC/POS bytes: `echo -e '\x1b\x40\x1b\x64\x03' | nc 192.168.1.100 9100`

### 10.6 Serial Connection Tips

- The default serial settings are 115200 baud, 8 data bits, no parity, 1 stop bit, hardware flow control.
- If using a USB-to-Serial adapter, ensure the adapter driver is installed and the correct COM port is assigned.
- On Linux, the serial device typically appears as `/dev/ttyUSB0` or `/dev/ttyS0`. Ensure proper permissions or add the user to the `dialout` group.

### 10.7 Cash Drawer Wiring

The cash drawer is driven through an RJ11 connector on the printer. Pin assignments:

| Pin | Function          |
|-----|-------------------|
| 1   | GND               |
| 2   | Drawer kick signal|
| 3   | Printer voltage sense |
| 4   | GND               |
| 5   | Drawer kick signal|
| 6   | GND               |

Pin 2 and Pin 5 correspond to the two selectable drawer connectors in the ESC p and DLE DC4 commands (m=0 for pin 2, m=1 for pin 5).

### 10.8 Firmware Reset via Button Combination

If the printer becomes unresponsive to software commands, a hardware reset can be performed:

1. Power off the printer.
2. Hold the FEED button.
3. Power on while holding FEED.
4. Wait for all indicator lights to cycle, then release.

This resets communication parameters but does not erase NV images.

### 10.9 Known Quirks

- **Auto-cut after ESC @:** Some firmware versions perform an unexpected partial cut on initialization. Send `GS V 1` explicitly to control cut behavior.
- **WiFi module sleep:** The HF-LPB100 WiFi module may enter sleep mode after prolonged inactivity, causing the first print job to be delayed or lost. Sending a keepalive packet (e.g., a single null byte) every 30 seconds can prevent this.
- **Ethernet buffer overflow:** When sending large print jobs over Ethernet (especially with NV images), insert small delays (~50 ms) between data chunks to prevent buffer overflow and garbled output.
- **Code page persistence:** The code page selected with ESC t is stored in volatile memory and resets on power cycle. If you need a non-default code page, send ESC t at the start of every print session.
