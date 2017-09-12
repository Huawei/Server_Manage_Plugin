/**
 * Created by licong on 2017/5/23.
 */

function getBootOrder() {
    return [{
        value: '0',
        label: 'BEV'
    }, {
        value: '1',
        label: 'Hard Disk Driver'
    }, {
        value: '2',
        label: 'CD/DVD-ROM Driver'
    }, {
        value: '3',
        label: 'Others'
    }];
}



var bios_en = {
    "AdvanceProcessor": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Power policy" },
                    "TIPS": "Set a power policy:Efficient: Prioritizes power saving.Performance: Prioritizes performance.Custom: Customizes a policy based on an Efficient or Performance policy.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Custom",
                        "INPUTID": "CustomPowerPolicy",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Efficient",
                                    "VALUE": "Efficient"
                                },
                                {
                                    "KEY": "Performance",
                                    "VALUE": "Performance"
                                },
                                {
                                    "KEY": "Custom",
                                    "VALUE": "Custom"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "EIST support" },
                    "TIPS": "Enhanced Intel SpeedStep Technology (EIST). EIST allows the system to dynamically adjust processor voltage and core frequency, which can result in decreased average power consumption and decreased average heat production.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "ProcessorEistEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Power saving" },
                    "TIPS": "Adjust the CPU P State to reduce power consumption:Enabled: Enables the adjustment of the CPU P State.Disabled: Disables the adjustment of the CPU P State.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "PowerSaving",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Intel HT technology" },
                    "TIPS": "Intel Hyper Threading Technology uses resources efficiently, enabling multiple threads to run on each core and increasing processor throughput.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "HTSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Console": {
        "CHILDREN": { "-cols": "2" }
    },
    "IPMI": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                "LABEL": { "ZH_CN": "Restore on AC Power Loss" },
                "TIPS": "Set how the BMC restores after an AC power failure.",
                "HTML": {
                    "INPUTTYPE": "select",
                    "DEFAULT": "Power On",
                    "INPUTID": "PowerStateRestoreOnACLoss",
                    "OPTIONS": {
                        "OPTION": [{
                                "KEY": "Power Off",
                                "VALUE": "Power Off"
                            },
                            {
                                "KEY": "Last State",
                                "VALUE": "Last State"
                            },
                            {
                                "KEY": "Power On",
                                "VALUE": "Power On"
                            },
                            {
                                "KEY": "Default",
                                "VALUE": "default"
                            }
                        ]
                    }
                }
            }]
        }
    },
    "Virtual": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "VT support" },
                    "TIPS": "Enable or disable the hardware-assisted Intel Virtualization Technology (VT).",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "VTSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "PCIe SR-IOV" },
                    "TIPS": "Enable or disable the hardware-assisted Intel Virtualization Technology (VT).",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "PCIeSRIOVSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_AdvanceProcessor": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Turbo mode" },
                    "TIPS": "The turbo mode allows a CPU to run faster than the base operating frequency.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "EnableTurboMode",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "ACPI version" },
                    "TIPS": "Advanced Configuration and Power Interface (ACPI) enables the operating system to set up and control the individual hardware components. ACPI supersedes both Power Management Plug and Play (PnP) and Advanced Power Management (APM). It delivers information about the battery, AC adapter, temperature, fan and system events, like “close lid” or “battery low.”",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "ACPI4.0",
                        "INPUTID": "AcpiVer",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "ACPI1.0B",
                                    "VALUE": "ACPI1.0B"
                                },
                                {
                                    "KEY": "ACPI3.0",
                                    "VALUE": "ACPI3.0"
                                },
                                {
                                    "KEY": "ACPI4.0",
                                    "VALUE": "ACPI4.0"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "NUMA" },
                    "TIPS": "Non-uniform memory access (NUMA) is a computer memory design used in multiprocessing, where the memory access time depends on the memory location relative to a processor. Under NUMA, a processor can access its own local memory faster than non-local memory (memory local to another processor or memory shared between processors).",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "NumaEn",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "C-states" },
                    "TIPS": "CPU C-State is a deep power down technology. C-States such as C3, C6, and C7 indicate power-saving effect in ascending order and processor recovery time in descending order.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "EVENT": "change",
                        "INPUTID": "ProcessorCcxEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:OsAcpiCx,EnCStates,EnableC3,EnableC6,EnableC7",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:OsAcpiCx,EnCStates,EnableC3,EnableC6,EnableC7",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "RasMode" },
                    "TIPS": "Memory reliability, availability, and serviceability (RAS) feature enhances error correction capability of memory. RAS has three channel modes:Independent: Each channel works independently. Data of each cache line comes from the same channel.Mirror: Generates mirrors for memory.Lock Step: Two memory channels work in a synchronous manner. They comprise a logical channel.Rank Spare: Backs up memory in the unit of rank.LockStep And RankSpare: Supports Lock Step and Rank Spare at the same time.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Independent",
                        "INPUTID": "RasMode",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Independent",
                                    "VALUE": "Independent"
                                },
                                {
                                    "KEY": "Mirror",
                                    "VALUE": "Mirror"
                                },
                                {
                                    "KEY": "LockStep",
                                    "VALUE": "LockStep"
                                },
                                {
                                    "KEY": "RankSpare",
                                    "VALUE": "RankSpare"
                                },
                                {
                                    "KEY": "LockStepAndRankSpare",
                                    "VALUE": "LockStep And RankSpare"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "OS ACPI Cx" },
                    "TIPS": "The operating system refers to certain advanced configuration and power interface (ACPI) Cx and informs the CPU of entering the C-state.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "ACPI C3",
                        "INPUTID": "OsAcpiCx",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "ACPI C3",
                                    "VALUE": "ACPI C3"
                                },
                                {
                                    "KEY": "ACPI C2",
                                    "VALUE": "ACPI C2"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "MLC spatial prefetcher" },
                    "TIPS": "Mid Level Cache (MLC) Spatial Prefetcher is used to prefetch two caches (128 bytes), halving the access time.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "MLCSpatialPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enhanced C-state" },
                    "TIPS": "Keeps P-states vary with C-states.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "EnCStates",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "MLC streamer prefetcher" },
                    "TIPS": "Mid Level Cache (MLC) Streamer Prefetcher is used to prefetch CPU instructions to shorten the time in executing instructions.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "MLCStreamerPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enable C3" },
                    "TIPS": "Closes all processor internal clocks, including bus interface (BI) and APIC.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "EnableC3",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "DCU IP prefetcher" },
                    "TIPS": "Data Cache Unit (DCU) IP Prefetcher is used to determine whether to prefetch data based on historical records, minimizing data access time.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "DCUIPPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enable C6" },
                    "TIPS": "Reduces the processor voltage to 0.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "EnableC6",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "DCU streamer prefetcher" },
                    "TIPS": "Data Cache Unit (DCU) Streamer Prefetcher is used to prefetch CPU data to shorten the time in accessing data.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "DCUStreamerPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enable C7" },
                    "TIPS": "Retains only the last thread and refresh the remaining last level cache (LLC).",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "EnableC7",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_Console": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Console serial redirect" },
                    "TIPS": "Map data from physical or virtual serial ports to system serial ports.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "EVENT": "change",
                        "INPUTID": "CREnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:CRTerminalType,CRParity,CRBaudRate,CRStopBits,CRDataBits",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:CRTerminalType,CRParity,CRBaudRate,CRStopBits,CRDataBits",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Terminal type" },
                    "TIPS": "Specify the protocol used by a terminal with serial ports.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "PC_ANSI",
                        "INPUTID": "CRTerminalType",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "VT_100",
                                    "VALUE": "VT_100"
                                },
                                {
                                    "KEY": "Vt_100+",
                                    "VALUE": "Vt_100+"
                                },
                                {
                                    "KEY": "VT_UTF8",
                                    "VALUE": "VT_UTF8"
                                },
                                {
                                    "KEY": "PC_ANSI",
                                    "VALUE": "PC_ANSI"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Parity" },
                    "TIPS": "Enable or disable parity check.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "None",
                        "INPUTID": "CRParity",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "None",
                                    "VALUE": "None"
                                },
                                {
                                    "KEY": "Even",
                                    "VALUE": "Even"
                                },
                                {
                                    "KEY": "Odd",
                                    "VALUE": "Odd"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Baud rate" },
                    "TIPS": "Set the baud rate, namely, the number of bits transmitted per second.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "115200",
                        "INPUTID": "CRBaudRate",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "115200",
                                    "VALUE": "115200"
                                },
                                {
                                    "KEY": "57600",
                                    "VALUE": "57600"
                                },
                                {
                                    "KEY": "19200",
                                    "VALUE": "19200"
                                },
                                {
                                    "KEY": "9600",
                                    "VALUE": "9600"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Stop bits" },
                    "TIPS": "The stop bits indicate the end of character. A larger number of bits indicates more tolerance of clock synchronization but lower data transfer.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "1",
                        "INPUTID": "CRStopBits",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "2",
                                    "VALUE": "2"
                                },
                                {
                                    "KEY": "1",
                                    "VALUE": "1"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Data bits" },
                    "TIPS": "Set the data bit width.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "8",
                        "INPUTID": "CRDataBits",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "8",
                                    "VALUE": "8"
                                },
                                {
                                    "KEY": "7",
                                    "VALUE": "7"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_IPMI": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "BMC WDT support for OS" },
                    "TIPS": "Set how the watchdog timer (WDT) responds to an operating system startup timeout.the Watchdog need the OS Watchdog driver support, as this may result in abnormal OS boot.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "EVENT": "change",
                        "INPUTID": "OSWdtEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:OSWdtTimeout,OSWdtAction",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:OSWdtTimeout,OSWdtAction",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": {
                        "ZH_CN": "BMC WDT time out for OS(min)",
                        "DISPLAY": "nodisplay"
                    },
                    "TIPS": "Set the maximum wait time of the watchdog timer (WDT) during an operating system startup.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "5",
                        "DISPLAY": "nodisplay",
                        "INPUTID": "OSWdtTimeout",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "2",
                                    "VALUE": "2"
                                },
                                {
                                    "KEY": "3",
                                    "VALUE": "3"
                                },
                                {
                                    "KEY": "4",
                                    "VALUE": "4"
                                },
                                {
                                    "KEY": "5",
                                    "VALUE": "5"
                                },
                                {
                                    "KEY": "6",
                                    "VALUE": "6"
                                },
                                {
                                    "KEY": "7",
                                    "VALUE": "7"
                                },
                                {
                                    "KEY": "8",
                                    "VALUE": "8"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": {
                        "ZH_CN": "BMC WDT action for OS",
                        "DISPLAY": "nodisplay"
                    },
                    "TIPS": "There are four options for the watchdog timer (WDT) responding to an operating system startup timeout.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Hard Reset",
                        "DISPLAY": "nodisplay",
                        "INPUTID": "OSWdtAction",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "No Action",
                                    "VALUE": "No Action"
                                },
                                {
                                    "KEY": "Hard Reset",
                                    "VALUE": "Hard Reset"
                                },
                                {
                                    "KEY": "Power Down",
                                    "VALUE": "Power Down"
                                },
                                {
                                    "KEY": "Power Cycle",
                                    "VALUE": "Power Cycle"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_Virtual": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "VT-D support" },
                    "TIPS": "Intel Virtualization Technology for Directed I/O (VT-D) implements the translation between virtual addresses and physical addresses in virtual environments, so as to enable direct memory access (DMA) remapping.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "EVENT": "change",
                        "INPUTID": "VTdSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:InterruptRemap,ATS,CoherencySupport,PassThroughDma",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:InterruptRemap,ATS,CoherencySupport,PassThroughDma",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Interrupt remap" },
                    "TIPS": "This feature enables a virtual device to generate interrupts for the CPU to process each interrupt.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "InterruptRemap",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "ATS support" },
                    "TIPS": "Address Translation Service (ATS) is a mechanism for PCIe buses. It works on a PCIe device. When a PCIe device sends transaction layer packets (TLPs) through address routing, ATS translates the destination address into a host physical address (HPA) to reduce the workload of Virtualization Technology for Directed I/O (VT-D). Additionally, ATS prevents devices in different domains from affecting each other.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "ATS",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Coherency support" },
                    "TIPS": "Consistency of support functions.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "CoherencySupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Pass through DMA support" },
                    "TIPS": "DMA is enabled through technology.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "PassThroughDma",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_Boot": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Quick Boot" },
                    "TIPS": "This feature enables a quick operating system startup by skipping some startup check items.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "QuickBoot",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Quiet Boot" },
                    "TIPS": "This feature enables the operating system to start in text mode.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "QuietBoot",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Boot Type" },
                    "TIPS": "Select an OS boot type for the BIOS: UEFI or legacy. DualBootType: supports two boot modes, namely, UEFI and legacy.LegacyBootType: supports only the legacy mode.UEFIBootType: supports only the UEFI mode.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Legacy Boot Type",
                        "INPUTID": "BootType",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "LegacyBootType",
                                    "VALUE": "Legacy Boot Type"
                                },
                                {
                                    "KEY": "DualBootType",
                                    "VALUE": "Dual Boot Type"
                                },
                                {
                                    "KEY": "UEFIBootType",
                                    "VALUE": "UEFI Boot Type"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "WakeOnLan" },
                    "TIPS": "Specifies whether to support WOL, which wakes up the server using a magic packet.",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "WakeOnPME",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Boot": { "order": "BEV,Hard Disk Driver,CD/DVD-ROM Driver,Others" }
}
var bios_zh_CN = {
    "AdvanceProcessor": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Power policy" },
                    "TIPS": "设置系统的能效方案:Efficient:省电方案，以省电为主。Performance:性能方案，以保证系统性能为主。Custom:客户化方案，已经设置相应的省电方案和性能方案。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Custom",
                        "INPUTID": "CustomPowerPolicy",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Efficient",
                                    "VALUE": "Efficient"
                                },
                                {
                                    "KEY": "Performance",
                                    "VALUE": "Performance"
                                },
                                {
                                    "KEY": "Custom",
                                    "VALUE": "Custom"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "EIST support" },
                    "TIPS": "增强型SpeedStep技术EIST(Enhanced Intel SpeedStep Technology)。当CPU使用频率较低时，通过动态的降低CPU工作频率，从而降低系统功耗以及发热；当监测到CPU使用率很高时，立即恢复到最初的工作频率。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "ProcessorEistEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Power saving" },
                    "TIPS": "CPU P State调节功能，通过调整CPU的P状态来减少能耗。Enabled:开启CPU P State调节功能。Disabled:关闭CPU P State调节功能。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "PowerSaving",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Intel HT technology" },
                    "TIPS": "Intel超线程技术(Intel Hyper Threading Technology)。该技术通过增加CPU内核的线程数以提高CPU性能。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "HTSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Console": {
        "CHILDREN": { "-cols": "2" }
    },
    "IPMI": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                "LABEL": { "ZH_CN": "Restore on AC Power Loss" },
                "TIPS": "设置BMC在AC掉电后的恢复模式。",
                "HTML": {
                    "INPUTTYPE": "select",
                    "DEFAULT": "Power On",
                    "INPUTID": "PowerStateRestoreOnACLoss",
                    "OPTIONS": {
                        "OPTION": [{
                                "KEY": "Power Off",
                                "VALUE": "Power Off"
                            },
                            {
                                "KEY": "Last State",
                                "VALUE": "Last State"
                            },
                            {
                                "KEY": "Power On",
                                "VALUE": "Power On"
                            },
                            {
                                "KEY": "Default",
                                "VALUE": "default"
                            }
                        ]
                    }
                }
            }]
        }
    },
    "Virtual": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "VT support" },
                    "TIPS": "启用或禁用硬件辅助虚拟化技术。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "VTSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "PCIe SR-IOV" },
                    "TIPS": "应用于虚拟化中的直通技术。通过SR-IOV，一个PCIe设备可以导出多个PCI物理功能，也可以导出共享该I/O设备上资源的一组虚拟功能，为每个虚拟机提供独立内存空间、中断和DMA (Direct Memory Access)流。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "PCIeSRIOVSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_AdvanceProcessor": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Turbo mode" },
                    "TIPS": "CPU加速模式，允许CPU的运行频率的比标称频率快。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "EnableTurboMode",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "ACPI version" },
                    "TIPS": "高级配置与电源接口ACPI(Advanced Configuration and Power Interface)，该功能可以通过操作系统对处理器、电池、嵌入式控制器等组件进行电源管理，使服务器满足一定性能的同时，降低功耗。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "ACPI4.0",
                        "INPUTID": "AcpiVer",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "ACPI1.0B",
                                    "VALUE": "ACPI1.0B"
                                },
                                {
                                    "KEY": "ACPI3.0",
                                    "VALUE": "ACPI3.0"
                                },
                                {
                                    "KEY": "ACPI4.0",
                                    "VALUE": "ACPI4.0"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "NUMA" },
                    "TIPS": "高级配置与电源接口ACPI(Advanced Configuration and Power Interface)，该功能可以通过操作系统对处理器、电池、嵌入式控制器等组件进行电源管理，使服务器满足一定性能的同时，降低功耗。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "NumaEn",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "C-states" },
                    "TIPS": "CPU C-State功能开关，CPU C-State是一项深度节能技术。其中，C3状态、C6状态、C7状态的节能效果逐渐依次增强，但CPU恢复到正常工作状态的时间依次增加。启用C-State功能后续设置如下选项:OS ACPI Cx:将某个ACPI Cx状态给操作系统作为参考，通知操作系统CPU可以进入C-State状态。Enhanced C-State:使P状态跟随C状态的变化而变化。Enable C3:用于关闭所有CPU内部时钟，包括总线接口和APIC。Enable C6:用于降低处理器电压至0。Enable C7:只保留最后的线程并刷新剩余LLC(Last Level Cache)。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "EVENT": "change",
                        "INPUTID": "ProcessorCcxEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:OsAcpiCx,EnCStates,EnableC3,EnableC6,EnableC7",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:OsAcpiCx,EnCStates,EnableC3,EnableC6,EnableC7",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "RasMode" },
                    "TIPS": "内存RAS功能用于增加内存的纠错性，确保内存的稳定性和正确性。它有三种通道模式:Independent:使各通道独立工作。每个Cache Line的数据来自同一个通道。Mirror:为内存做镜像。Lock Step模式:使两个内存通道以完全一致的步调工作，两个物理通道组成一个逻辑通道。Rank Spare模式:以Rank条为单位进行内存备份。Lock Step & Rank Spare:同时支持Lock Step & Rank Spare模式。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Independent",
                        "INPUTID": "RasMode",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Independent",
                                    "VALUE": "Independent"
                                },
                                {
                                    "KEY": "Mirror",
                                    "VALUE": "Mirror"
                                },
                                {
                                    "KEY": "LockStep",
                                    "VALUE": "LockStep"
                                },
                                {
                                    "KEY": "RankSpare",
                                    "VALUE": "RankSpare"
                                },
                                {
                                    "KEY": "LockStepAndRankSpare",
                                    "VALUE": "LockStep And RankSpare"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "OS ACPI Cx" },
                    "TIPS": "将某个ACPI cx状态给操作系统作为参考，通知操作系统CPU可以进入C state",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "ACPI C3",
                        "INPUTID": "OsAcpiCx",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "ACPI C3",
                                    "VALUE": "ACPI C3"
                                },
                                {
                                    "KEY": "ACPI C2",
                                    "VALUE": "ACPI C2"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "MLC spatial prefetcher" },
                    "TIPS": "MLC(Mid Level Cache) Spatial预读取功能用于预读取两个高速缓冲存储器(128 bytes)，读取时间是平时预读取量的两倍。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "MLCSpatialPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enhanced C-state" },
                    "TIPS": "使P状态跟随C状态的变化而变化。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "EnCStates",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "MLC streamer prefetcher" },
                    "TIPS": "MLC(Mid Level Cache) Streamer预读取功能用于预读取CPU的指令，减少指令读取时间。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "MLCStreamerPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enable C3" },
                    "TIPS": "关闭所有CPU内部时钟，包括总线接口和APIC。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "EnableC3",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "DCU IP prefetcher" },
                    "TIPS": "DCU(Data Cache Unit)IP预读取功能用于从历史记录中判断是否有数据需要预读取，从而减少数据的读取时间。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "DCUIPPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enable C6" },
                    "TIPS": "可以降低处理器电压至0。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "EnableC6",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "DCU streamer prefetcher" },
                    "TIPS": "DCU(Data Cache Unit)Streamer预读取功能用于预读取CPU的数据，从而减少数据的读取时间。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "DCUStreamerPrefetcherEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Enable C7" },
                    "TIPS": "仅保留最后的线程刷新剩余LLC。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "EnableC7",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_Console": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Console serial redirect" },
                    "TIPS": "串将指定的物理串口或虚拟串口的数据映射到系统串口。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "EVENT": "change",
                        "INPUTID": "CREnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:CRTerminalType,CRParity,CRBaudRate,CRStopBits,CRDataBits",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:CRTerminalType,CRParity,CRBaudRate,CRStopBits,CRDataBits",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Terminal type" },
                    "TIPS": "串口终端的协议类型。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "PC_ANSI",
                        "INPUTID": "CRTerminalType",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "VT_100",
                                    "VALUE": "VT_100"
                                },
                                {
                                    "KEY": "Vt_100+",
                                    "VALUE": "Vt_100+"
                                },
                                {
                                    "KEY": "VT_UTF8",
                                    "VALUE": "VT_UTF8"
                                },
                                {
                                    "KEY": "PC_ANSI",
                                    "VALUE": "PC_ANSI"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Parity" },
                    "TIPS": "奇偶校验功能开关，该功能可以校验代码传输正确性。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "None",
                        "INPUTID": "CRParity",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "None",
                                    "VALUE": "None"
                                },
                                {
                                    "KEY": "Even",
                                    "VALUE": "Even"
                                },
                                {
                                    "KEY": "Odd",
                                    "VALUE": "Odd"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Baud rate" },
                    "TIPS": "设置串口波特率，表示每秒钟传送的bit个数。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "115200",
                        "INPUTID": "CRBaudRate",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "115200",
                                    "VALUE": "115200"
                                },
                                {
                                    "KEY": "57600",
                                    "VALUE": "57600"
                                },
                                {
                                    "KEY": "19200",
                                    "VALUE": "19200"
                                },
                                {
                                    "KEY": "9600",
                                    "VALUE": "9600"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Stop bits" },
                    "TIPS": "停止位表示单个数据包的最后一位。停止位的位数越多，不同时钟同步的容忍程度越大，但是数据传输率同时也越慢。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "1",
                        "INPUTID": "CRStopBits",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "2",
                                    "VALUE": "2"
                                },
                                {
                                    "KEY": "1",
                                    "VALUE": "1"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Data bits" },
                    "TIPS": "设置串口数据位宽，表示通信中实际的数据位。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "8",
                        "INPUTID": "CRDataBits",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "8",
                                    "VALUE": "8"
                                },
                                {
                                    "KEY": "7",
                                    "VALUE": "7"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_IPMI": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "BMC WDT support for OS" },
                    "TIPS": "在OS启动过程超时后，设置支持WDT(watchdog timer)响应动作。看门狗需要OS看门狗驱动支持，否则可能会导致OS启动异常。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "EVENT": "change",
                        "INPUTID": "OSWdtEnable",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:OSWdtTimeout,OSWdtAction",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:OSWdtTimeout,OSWdtAction",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": {
                        "ZH_CN": "BMC WDT time out for OS(min)",
                        "DISPLAY": "nodisplay"
                    },
                    "TIPS": "设置OS启动过程中，WDT(watchdog timer)等待的最长时间。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "5",
                        "DISPLAY": "nodisplay",
                        "INPUTID": "OSWdtTimeout",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "2",
                                    "VALUE": "2"
                                },
                                {
                                    "KEY": "3",
                                    "VALUE": "3"
                                },
                                {
                                    "KEY": "4",
                                    "VALUE": "4"
                                },
                                {
                                    "KEY": "5",
                                    "VALUE": "5"
                                },
                                {
                                    "KEY": "6",
                                    "VALUE": "6"
                                },
                                {
                                    "KEY": "7",
                                    "VALUE": "7"
                                },
                                {
                                    "KEY": "8",
                                    "VALUE": "8"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": {
                        "ZH_CN": "BMC WDT action for OS",
                        "DISPLAY": "nodisplay"
                    },
                    "TIPS": "在 OS启动过程超时后，WDT的响应方式。有以下四种选项:无响应: WDT不做任何操作。强制复位: 强行将系统复位。下电: 将系统下电。 循环重启: 不断尝试启动操作系统， 直至可以正常进入系统工作。 ",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Hard Reset",
                        "DISPLAY": "nodisplay",
                        "INPUTID": "OSWdtAction",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "No Action",
                                    "VALUE": "No Action"
                                },
                                {
                                    "KEY": "Hard Reset",
                                    "VALUE": "Hard Reset"
                                },
                                {
                                    "KEY": "Power Down",
                                    "VALUE": "Power Down"
                                },
                                {
                                    "KEY": "Power Cycle",
                                    "VALUE": "Power Cycle"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_Virtual": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "VT-D support" },
                    "TIPS": "Intel虚拟化数据地址转换技术。用于在虚拟化场景下，实现虚拟地址和真实物理地址的内部转换，从而实现DMA重映射。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "EVENT": "change",
                        "INPUTID": "VTdSupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "CONTROLL": "show:InterruptRemap,ATS,CoherencySupport,PassThroughDma",
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "CONTROLL": "hide:InterruptRemap,ATS,CoherencySupport,PassThroughDma",
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Interrupt remap" },
                    "TIPS": "用于使虚拟设备生成不同的中断，便于CPU处理各个中断信号。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "InterruptRemap",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "ATS support" },
                    "TIPS": "ATS是PCIe总线的一个机制。它在PCIe设备中实现。具体实现形式是PCIe设备使用地址路由方式发送TLP时，就进行地址转换，转换为HPA地址，从而减轻VT-d的工作负担。另外，它还可以避免不同域中的设备互相影响。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "ATS",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Coherency support" },
                    "TIPS": "一致性支持功能。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "CoherencySupport",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Pass through DMA support" },
                    "TIPS": "是否使能DMA直通技术。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "PassThroughDma",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Property_Boot": {
        "CHILDREN": {
            "-cols": "2",
            "CHILD": [{
                    "LABEL": { "ZH_CN": "Quick Boot" },
                    "TIPS": "在启动的过程中跳过一些检测步骤，快速启动操作系统，以此缩短系统启动时间。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Enabled",
                        "INPUTID": "QuickBoot",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Quiet Boot" },
                    "TIPS": "以本文方式启动操作系统。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "QuietBoot",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "Boot Type" },
                    "TIPS": "选择BIOS启动类型，支持UEFI和Legacy BIOS启动。Dual Boot Type:支持UEFI和Legacy的两种启动引导方式。Legacy Boot Type:仅支持Legacy启动引导方式。UEFI Boot Type:仅支持UEFI启动引导方式。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Legacy Boot Type",
                        "INPUTID": "BootType",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "LegacyBootType",
                                    "VALUE": "Legacy Boot Type"
                                },
                                {
                                    "KEY": "DualBootType",
                                    "VALUE": "Dual Boot Type"
                                },
                                {
                                    "KEY": "UEFIBootType",
                                    "VALUE": "UEFI Boot Type"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                },
                {
                    "LABEL": { "ZH_CN": "WakeOnLan" },
                    "TIPS": "是否支持WOL启动选项，支持网络魔幻包唤醒服务器。",
                    "HTML": {
                        "INPUTTYPE": "select",
                        "DEFAULT": "Disabled",
                        "INPUTID": "WakeOnPME",
                        "OPTIONS": {
                            "OPTION": [{
                                    "KEY": "Enabled",
                                    "VALUE": "Enabled"
                                },
                                {
                                    "KEY": "Disabled",
                                    "VALUE": "Disabled"
                                },
                                {
                                    "KEY": "Default",
                                    "VALUE": "default"
                                }
                            ]
                        }
                    }
                }
            ]
        }
    },
    "Boot": { "order": "BEV,Hard Disk Driver,CD/DVD-ROM Driver,Others" }
}

function getBiosData() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return bios_en;
        }
    }
    return bios_zh_CN;
}