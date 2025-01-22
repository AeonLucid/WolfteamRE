const baseAddress = Module.getBaseAddress('WolfTeam.exe');
const idaBases: any = {
    'Wolfteam.exe': '0x400000'
};

function threadBacktraceMapper(ptr: NativePointer) {
    const module = Process.findModuleByAddress(ptr);
    if (module !== null) {
        const idaBase = idaBases[module.name];
        if (idaBase !== undefined) {
            return `${module.name}!${ptr.sub(module.base).add(idaBase)} [ida]`;
        }

        return `${module.name}!${ptr.sub(module.base)}`;
    }

    return DebugSymbol.fromAddress(ptr);
}

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
            console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(threadBacktraceMapper).join('\n'));
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
    // Interceptor.attach(Module.findExportByName('ws2_32.dll', 'recv')!, {
    //     onEnter: function (args) {
    //         const s = args[0].toInt32();
    //         const buf = args[1];
    //         const len = args[2].toInt32();
    //         const flags = args[3].toInt32();
    
    //         console.log(`:: recv(s: ${s}, buf: ${buf}, len: ${len}, flags: ${flags})`);
    //         // console.log(Thread.backtrace(this.context, Backtracer.ACCURATE).map(DebugSymbol.fromAddress).join('\n'));
    //     },
    //     onLeave: function (retval) {
    //         console.log(`:: recv returned ${retval}`);
    //     }
    // });

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
    const clientShellBaseEnd = clientShellBase.add(Process.findModuleByName(fileName)!.size);
    const clientShellBaseIda = ptr('0x793A0000');
    const clientShellAddress = (idaOffset: string) => clientShellBase.add(ptr(idaOffset).sub(clientShellBaseIda));

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

    // Hook log 0x793FA2D0
    Interceptor.attach(clientShellAddress('0x793FA2D0'), {
        onEnter: function (args) {
            const msg = args[1].readUtf8String();

            console.log(`:: log(msg: ${msg})`);
        }
    });

    // Hook 0x7942E970
    // Interceptor.attach(clientShellAddress('0x7942E970'), {
    //     onEnter: function (args) {
    //         const context = this.context as Ia32CpuContext;

    //         console.log(`:: eax eax eax eax eax ${context.eax.sub(clientShellBase).add(clientShellBaseIda)}`);
    //     }
    // });

    // Hook 0x794CF420(ptr, ptr)
    Interceptor.attach(clientShellAddress('0x794CF420'), {
        onEnter: function (args) {
            console.log(`:: 0x794CF420(ptr: ${args[0]}, ptr: ${args[1]}, ptr: ${args[2]})`);
            console.log(hexdump(args[0], { length: 32 }));
            // console.log(hexdump(args[1], { length: 0x16 }));
        }
    });

    // Hook EncryptPacket(ptr, ptr)
    Interceptor.attach(clientShellAddress('0x794CD0A0'), {
        onEnter: function (args) {
            const context = this.context as Ia32CpuContext;
            const packetData = context.ecx;
            const packetDataLen = packetData.add(0x2000).readU32();
            const packetSeq = args[0].toInt32();

            console.log('==============================================================================');
            console.log(`:: EncryptPacket(packetData: ${packetData}, packetDataLen: ${packetDataLen}, ${packetSeq})`);
            console.log(hexdump(packetData, { length: packetDataLen }));

            this.packetData = packetData;
        },
        onLeave: function (retval) {
            const packetData = this.packetData;
            const packetDataLen = packetData.add(0x2000).readU32();

            console.log(`:: EncryptPacket result, packetLen: ${packetDataLen}`);
            console.log(hexdump(packetData, { length: packetDataLen }));
            console.log('==============================================================================');
        }
    });

    // Hook EncryptPacketAes(ptr, ptr)
    Interceptor.attach(clientShellAddress('0x795DA200'), {
        onEnter: function (args) {
            const uk0 = args[0];
            const uk1 = args[1];

            this.uk0 = uk0;

            console.log(`:: aes_stuff(uk0: ${uk0}, uk1: ${uk1})`);
            console.log(hexdump(uk0.sub(8), { length: 0x10 }));
            // console.log(hexdump(uk1, { length: 0x10 }));
        },
        // onLeave: function (retval) {
        //     console.log(`:: EncryptPacketAes returned ${retval}`);
        //     console.log(hexdump(this.uk0, { length: 0x10 }));
        // }
    });

    // Hook 0x794CC780(ptr, ptr, ptr)
    Interceptor.attach(clientShellAddress('0x794CC780'), {
        onEnter: function (args) {
            // fastcall
            const unknown = args[2];
            const packetData = args[0];
            const packetLen = args[1].toInt32();

            console.log(`:: 0x794CC780(unknown: ${unknown}, packetData: ${packetData}, packetLen: ${packetLen})`);
        }
    });

    // Hook SetupAes(key, keySize, id, ctx)
    Interceptor.attach(clientShellAddress('0x796926D0'), {
        onEnter: function (args) {
            const key = args[0];
            const keySize = args[1].toInt32();
            const id = args[2].toInt32();
            const ctx = args[3];

            console.log(`:: SetupAes(key: ${key}, keySize: ${keySize}, id: ${id}, ctx: ${ctx})`);
            console.log(hexdump(key, { length: keySize }));
        }
    });

    // Hook BlowfishEncrypt(ptr, ptr, ptr)
    Interceptor.attach(clientShellAddress('0x79522240'), {
        onEnter: function (args) {
            const context = this.context as Ia32CpuContext;
            const blowfishCtx = context.ecx;
            const blowfishBox = args[0].toInt32();
            const packetData = args[1];
            const packetDataLen = packetData.add(0x2000).readU32();

            console.log(`:: BlowfishEncrypt(blowfishCtx: ${blowfishCtx}, blowfishBox: ${blowfishBox}, packetData: ${packetData})`);
            console.log(hexdump(packetData, { length: packetDataLen }));

            Stalker.follow(this.threadId, {
                transform: function (iterator: StalkerX86Iterator) {
                    let inst = iterator.next();
                    if (inst == null) {
                        return;
                    }
                
                    do {
                        let instruction = inst as X86Instruction;
                        let instAddr = instruction.address;
                
                        // const appAddress = instruction.address;
                        // const appCode = appAddress.compare(clientShellBase) >= 0 && appAddress.compare(clientShellBaseEnd) === -1;
                
                        // if (!appCode) {
                        //     iterator.keep();
                        //     continue;
                        // }
                
                        // Log.
                        if (instAddr.equals(clientShellAddress('0x79668232'))) {
                            iterator.putCallout(function (ctx: CpuContext) {
                                let context = ctx as Ia32CpuContext;
                                let value = context.eax.add(context.esi.toInt32() * 4).readU32();

                                console.log("[Found value 1]", value.toString(16));
                            });
                        }

                        if (instAddr.equals(clientShellAddress('0x79668270'))) {
                            iterator.putCallout(function (ctx: CpuContext) {
                                let context = ctx as Ia32CpuContext;
                                let value = context.edi.add(context.esi.toInt32() * 4).add(4).readU32();

                                console.log("[Found value 2]", value.toString(16));
                            });
                        }

                        if (instAddr.equals(clientShellAddress('0x796685D7'))) {
                            iterator.putCallout(function (ctx: CpuContext) {
                                let context = ctx as Ia32CpuContext;

                                console.log("[Found value 1]", context.edx.toString(16));
                                console.log("[Found value 2]", context.edi.toString(16));
                            });
                        }

                        if (instAddr.equals(clientShellAddress('0x796685C1'))) {
                            iterator.putCallout(function (ctx: CpuContext) {
                                let context = ctx as Ia32CpuContext;
                                let value = context.ecx.add(0x44).readU32();

                                console.log("[Found this->PBox[17]]", value);
                            });
                        }
                
                        // Continue.
                        iterator.keep();
                    } while ((inst = iterator.next()) !== null);
                },
            });
        },
        onLeave: function (retval) {
            console.log(`:: BlowfishEncrypt returned ${retval}`);

            Stalker.unfollow(this.threadId);
        }
    });

    // Dump blowfish tables
    // Interceptor.attach(clientShellAddress('0x79668210'), {
    //     onEnter: function (args) {
    //         const context = this.context as Ia32CpuContext;
    //         const blowfishEntry = context.ecx;

    //         const PBox = [];
    //         const SBox = [];
    //         const SBox2 = [];
    //         const SBox3 = [];
    //         const SBox4 = [];

    //         for (let i = 0; i < 18; i++) {
    //             PBox.push('0x' + blowfishEntry.add(i * 4).readU32().toString(16));
    //         }

    //         for (let i = 0; i < 256; i++) {
    //             SBox.push('0x' + blowfishEntry.add(0x48).add(i * 4).readU32().toString(16));
    //         }

    //         for (let i = 0; i < 256; i++) {
    //             SBox2.push('0x' + blowfishEntry.add(0x448).add(i * 4).readU32().toString(16));
    //         }

    //         for (let i = 0; i < 256; i++) {
    //             SBox3.push('0x' + blowfishEntry.add(0x848).add(i * 4).readU32().toString(16));
    //         }

    //         for (let i = 0; i < 256; i++) {
    //             SBox4.push('0x' + blowfishEntry.add(0xC48).add(i * 4).readU32().toString(16));
    //         }

    //         console.log(`:: BlowfishEncryptEntry PBox: ${PBox}`);
    //         console.log(`:: BlowfishEncryptEntry SBox: ${SBox}`);
    //         console.log(`:: BlowfishEncryptEntry SBox2: ${SBox2}`);
    //         console.log(`:: BlowfishEncryptEntry SBox3: ${SBox3}`);
    //         console.log(`:: BlowfishEncryptEntry SBox4: ${SBox4}`);
    //     }
    // });

    Interceptor.attach(clientShellAddress('0x79522220'), {
        onEnter: function (args) {
            console.log(`:: BlowfishKey(${args[1].readCString()})`);
        }
    });
}

// CS_BR_CHAINLIST_REQ

function main() {
    if (started) {
        return;
    }

    started = true;

    // Start.
    console.log(':: Starting agent...');
}

console.log('BaseAddress', baseAddress);

hookGeneric();