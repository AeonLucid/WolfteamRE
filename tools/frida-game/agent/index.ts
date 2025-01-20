const baseAddress = Module.getBaseAddress('WolfTeam.exe');

function hookGeneric() {
    // Hook ClipCursor
    Interceptor.attach(Module.findExportByName('user32.dll', 'ClipCursor')!, {
        onEnter: function (args) {
            args[0] = ptr(0);

            // console.log(`:: ClipCursor replaced with ClipCursor(NULL)`);
        }
    });

    // Hook method to prevent HWND_TOP from being used.
    Interceptor.attach(Module.findExportByName('user32.dll', 'SetWindowPos')!, {
        onEnter: function (args) {
            const hWnd = args[0].toInt32();
            const hWndInsertAfter = args[1].toInt32();

            if (hWndInsertAfter === -1) {
                args[1] = ptr(0);
            }

            // console.log(`:: SetWindowPos(hWnd: ${hWnd}, hWndInsertAfter: ${hWndInsertAfter})`);
        }
    });

    // Hook FindWindowA
    Interceptor.attach(Module.findExportByName('user32.dll', 'FindWindowA')!, {
        onEnter: function (args) {
            const lpClassName = args[0].isNull() ? null : args[0].readAnsiString();
            const lpWindowName = args[1].isNull() ? null : args[1].readAnsiString();
    
            // console.log(`:: FindWindowA(lpClassName: ${lpClassName}, lpWindowName: ${lpWindowName})`);
    
            this.lpClassName = lpClassName;
            this.lpWindowName = lpWindowName;
        },
        onLeave: function (retval) {
            if (this.lpClassName !== null && this.lpClassName === 'PROCMON_WINDOW_CLASS') {
                retval.replace(ptr(0));
            }
    
            if (this.lpWindowName !== null && this.lpWindowName.includes('sysinternals.com')) {
                retval.replace(ptr(0));
            }
    
            // console.log(`:: FindWindowA returned ${retval}`);
        }
    });

    // Hook ws32 connect
    Interceptor.attach(Module.findExportByName('ws2_32.dll', 'connect')!, {
        onEnter: function (args) {
            const s = args[0].toInt32();
            const name = args[1];
            const namelen = args[2].toInt32();
    
            console.log(`:: connect(s: ${s}, name: ${name}, namelen: ${namelen})`);
            // console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(DebugSymbol.fromAddress).join('\n'));
        },
        onLeave: function (retval) {
            console.log(`:: connect returned ${retval}`);
        }
    });
    
    // Hook ws32 send
    Interceptor.attach(Module.findExportByName('ws2_32.dll', 'send')!, {
        onEnter: function (args) {
            const s = args[0].toInt32();
            const buf = args[1];
            const len = args[2].toInt32();
            const flags = args[3].toInt32();
    
            console.log(`:: send(s: ${s}, buf: ${buf}, len: ${len}, flags: ${flags})`);
            // console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(DebugSymbol.fromAddress).join('\n'));
        },
        onLeave: function (retval) {
            console.log(`:: send returned ${retval}`);
        }
    });

    // Hook ws32 sendto
    Interceptor.attach(Module.findExportByName('ws2_32.dll', 'sendto')!, {
        onEnter: function (args) {
            const s = args[0].toInt32();
            const buf = args[1];
            const len = args[2].toInt32();
            const flags = args[3].toInt32();
            const to = args[4];
            const tolen = args[5].toInt32();
    
            console.log(`:: sendto(s: ${s}, buf: ${buf}, len: ${len}, flags: ${flags}, to: ${to}, tolen: ${tolen})`);
            // console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(DebugSymbol.fromAddress).join('\n'));
        },
        onLeave: function (retval) {
            console.log(`:: sendto returned ${retval}`);
        }
    });
    
    // Hook ws32 recv
    Interceptor.attach(Module.findExportByName('ws2_32.dll', 'recv')!, {
        onEnter: function (args) {
            const s = args[0].toInt32();
            const buf = args[1];
            const len = args[2].toInt32();
            const flags = args[3].toInt32();
    
            console.log(`:: recv(s: ${s}, buf: ${buf}, len: ${len}, flags: ${flags})`);
            // console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(DebugSymbol.fromAddress).join('\n'));
        },
        onLeave: function (retval) {
            console.log(`:: recv returned ${retval}`);
        }
    });

    // Hook LoadLibraryA
    // Interceptor.attach(Module.findExportByName('kernel32.dll', 'LoadLibraryA')!, {
    //     onEnter: function (args) {
    //         const lpLibFileName = args[0].isNull() ? null : args[0].readAnsiString();
    //         this.lpLibFileName = lpLibFileName;
    //     },
    //     onLeave: function (retval) {
    //         const fileName = this.lpLibFileName.split('\\').pop();
    //         if (fileName.includes('csh') && fileName.endsWith('.tmp')) {
    //             loadedClientShell(fileName);
    //         }
    //     }
    // });

    // Hook LdrLoadDll.
    Interceptor.attach(Module.findExportByName('ntdll.dll', 'LdrLoadDll')!, {
        onEnter: function (args) {
            this.namePtr = args[2];
            this.basePtr = args[3];
        },
        onLeave: function (retval) {
            const name: string = this.namePtr.add(4).readPointer().readUtf16String();
            const base: NativePointer = this.basePtr.readPointer();

            const fileName = name.split('\\').pop()!;
            if (fileName.startsWith('csh') && fileName.endsWith('.tmp')) {
                loadedClientShell(name);
            }
        }
    });

    // Hook dns lookup
    Interceptor.attach(Module.findExportByName('ws2_32.dll', 'gethostbyname')!, {
        onEnter: function (args) {
            const name = args[0].readAnsiString();
    
            console.log(`:: gethostbyname(name: ${name})`);
        },
        onLeave: function (retval) {
            console.log(`:: gethostbyname returned ${retval}`);
        }
    });

    // Hook ShellExecuteExW
    // Interceptor.attach(Module.findExportByName('shell32.dll', 'ShellExecuteExW')!, {
    //     onEnter: function (args) {
    //         const lpExecInfo = args[0];
    
    //         const lpFile = lpExecInfo.add(0x10).readPointer().readUtf16String();
    //         const lpParameters = lpExecInfo.add(0x18).readPointer().readUtf16String();
    //         const lpDirectory = lpExecInfo.add(0x20).readPointer().readUtf16String();
    
    //         console.log(`:: ShellExecuteExW(lpFile: ${lpFile}, lpParameters: ${lpParameters}, lpDirectory: ${lpDirectory})`);
    //         console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(ptr => {
    //             const module = Process.findModuleByAddress(ptr);
    //             if (module !== null) {
    //                 return `${module.name}!${ptr.sub(module.base)}`;
    //             }

    //             return DebugSymbol.fromAddress(ptr);
    //         }).join('\n'));
    //     },
    //     onLeave: function (retval) {
    //         console.log(`:: ShellExecuteExW returned ${retval}`);
    //     }
    // });

    // Hook CreateProcessW
    // Interceptor.attach(Module.findExportByName('kernel32.dll', 'CreateProcessW')!, {
    //     onEnter: function (args) {
    //         const lpApplicationName = args[0].isNull() ? null : args[0].readUtf16String();
    //         const lpCommandLine = args[1].isNull() ? null : args[1].readUtf16String();
    
    //         console.log(`:: CreateProcessW(lpApplicationName: ${lpApplicationName}, lpCommandLine: ${lpCommandLine})`);
    //         console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(ptr => {
    //             // const moduleMap = new ModuleMap();
    //             // const module = moduleMap.find(ptr);
    //             // if (module !== null) {
    //             //     return `${module.name}!${ptr.sub(module.base)}`;
    //             // }

    //             return DebugSymbol.fromAddress(ptr);
    //         }).join('\n'));
    //     },
    //     onLeave: function (retval) {
    //         console.log(`:: CreateProcessW returned ${retval}`);
    //     }
    // });
}

let started = false;

function loadedClientShell(fileName: string) { 
    const clientShellBase = Module.findBaseAddress(fileName)!;
    const clientShellAddress = (idaOffset: string) => clientShellBase.add(ptr(idaOffset).sub(ptr('0x793A0000')));

    console.log(`:: Loaded ClientShell: ${fileName} at ${clientShellBase}`);

    Interceptor.replace(clientShellAddress('0x794CCEA0'), new NativeCallback(function() {
        console.log(':: ClientShell Load XIGNCODE');
        return ptr(1);
    }, 'pointer', []));

    const BrokerServerIP = Memory.allocUtf8String('127.0.0.1');
    const BrokerServerPort = ptr(40706);
    
    // Hook GetProp_Str(ptr, category, key) 0x793E90B0
    Interceptor.attach(clientShellAddress('0x793E90B0'), {
        onEnter: function (args) {
            const ptr = args[2];
            const category = args[0].readCString();
            const key = args[1].readCString();
    
            console.log(`:: GetProp_Str(ptr: ${ptr}, category: ${category}, key: ${key})`);

            this.category = category;
            this.key = key;
        },
        onLeave: function (retval) {
            console.log(`:: GetProp_Str returned ${retval} [${retval.readCString()}]`);

            if (this.category === 'BrokerServer' && this.key === 'IP') {
                console.log(`:: Replacing BrokerServer IP with 127.0.0.1`);
                retval.replace(BrokerServerIP);
            }
        }
    });

    // Hook GetProp_Int(ptr, category, key) 0x793E8CB0
    Interceptor.attach(clientShellAddress('0x793E8CB0'), {
        onEnter: function (args) {
            const ptr = args[2];
            const category = args[0].readCString();
            const key = args[1].readCString();
    
            console.log(`:: GetProp_Int(ptr: ${ptr}, category: ${category}, key: ${key})`);

            this.category = category;
            this.key = key;
        },
        onLeave: function (retval) {
            console.log(`:: GetProp_Int returned ${retval} [${retval.toInt32()}]`);

            if (this.category === 'BrokerServer' && this.key === 'Port') {
                console.log(`:: Replacing BrokerServer Port with 40706`);
                retval.replace(BrokerServerPort);
            }
        }
    });

    const EmptyString = Memory.allocUtf8String('');

    // Hook GetStringFromStringTable
    Interceptor.attach(clientShellAddress('0x793FECC0'), {
        onEnter: function (args) {
            const stringId = args[0].toInt32();

            // console.log(`:: GetStringFromStringTable(stringId: ${stringId})`);

            this.stringId = stringId;
        },
        onLeave: function (retval) {
            if (this.stringId === 1904) {
                // console.log(`:: Replacing string with empty string`);
                retval.replace(EmptyString);
            }
        }
    });
}

function waitForUnpack() {
    // const intervalHandle = setInterval(() => {
    //     if (ptr(0x43DF10).readU8() === 0x6A) {
    //         clearInterval(intervalHandle);

    //         main();
    //     }
    // }, 100);
}

function main() {
    if (started) {
        return;
    }

    started = true;

    // Start.
    console.log(':: Starting agent...');
}

console.log('BaseAddress', baseAddress);

waitForUnpack();
hookGeneric();