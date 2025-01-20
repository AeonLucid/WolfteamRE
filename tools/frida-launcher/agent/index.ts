const baseAddress = Module.getBaseAddress('NyxLauncher.exe');

console.log(baseAddress);

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

let started = false;

function waitForUnpack() {
    const intervalHandle = setInterval(() => {
        if (ptr(0x43DF10).readU8() === 0x6A) {
            clearInterval(intervalHandle);

            main();
        }
    }, 100);
}

// const hashData = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa';
// const hashDataPtr = Memory.allocUtf8String(hashData);

function main() {
    if (started) {
        return;
    }

    started = true;

    // Start.
    console.log(':: Starting agent...');

    Interceptor.attach(ptr(0x439890), {
        onEnter: function (args) {
            const pFormat = args[0].readAnsiString();
            const pLine = args[1].readAnsiString();

            console.log(`:: NyxLauncher::Log1 ${pFormat?.trimEnd()}`);
        }
    })

    Interceptor.attach(ptr(0x4399E0), {
        onEnter: function (args) {
            const pFormat = args[0].readAnsiString();

            console.log(`:: NyxLauncher::Log2 ${pFormat?.trimEnd()}`);
        }
    })

    Interceptor.attach(ptr(0x43DF10), {
        onEnter: function (args) {
            const pCategory = args[0].readAnsiString();
            const pKey = args[1].readAnsiString();

            console.log(`:: NyxLauncher::GetProp(${pCategory}, ${pKey})`);
            // console.log(Thread.backtrace(this.context, Backtracer.FUZZY).map(DebugSymbol.fromAddress).join('\n'));
        },
        onLeave: function (retval) {
            console.log(`:: NyxLauncher::GetProp returned ${retval.readAnsiString()}`);
        }
    })

    // aes_init
    Interceptor.attach(ptr(0x44DD40), {
        onEnter: function (args) {
            const pKey = args[0];
            const pKeyLen = args[1].toInt32();
            const pUk2 = args[2].toInt32();
            const pCtx = args[3];

            console.log(`:: NyxLauncher::AesInit(pKey ${pKey}, pKeyLen ${pKeyLen}, pUk2 ${pUk2}, pCtx ${pCtx})`);
            console.log(hexdump(pKey, { length: pKeyLen }));
        }
    })

    // sha1_init
    Interceptor.attach(ptr(0x45C570), {
        onEnter: function (args) {
            const pCtx = args[0];

            for (let index = 0; index < 0x60; index++) {
                pCtx.add(index).writeU8(0);
            }
        }
    });

    // sha1_append 0x461C00
    Interceptor.attach(ptr(0x461C00), {
        onEnter: function (args) {
            // args[1] = hashDataPtr;
            // args[2] = ptr(hashData.length);

            const pCtx = args[0];
            const pBuf = args[1];
            const pLen = args[2].toInt32();

            console.log('========================================================================');
            console.log(`:: NyxLauncher::Sha1Append(pCtx ${pCtx}, pBuf ${pBuf}, pLen ${pLen})`);
            console.log(hexdump(pBuf, { length: pLen }));

            // console.log('================== ctx before');
            // console.log(hexdump(pCtx, { length: 0x60 }));

            this.pCtx = pCtx;
        },
        onLeave: function (retval) {
            // console.log('================== ctx after');
            // console.log(hexdump(this.pCtx, { length: 0x60 }));
        }
    });

    // sha1_final 0x45C5B0
    Interceptor.attach(ptr(0x45C5B0), {
        onEnter: function (args) {
            const pCtx = args[0];

            console.log(`:: NyxLauncher::Sha1Final(pCtx ${pCtx}, ..)`);
            // console.log('================== ctx before');
            // console.log(hexdump(pCtx, { length: 0x60 }));

            this.pCtx = pCtx;
        },
        onLeave: function (retval) {
            // console.log('================== ctx after');
            // console.log(hexdump(this.pCtx, { length: 0x60 }));
        }
    });

    // md5(in, out, len) 0x448D80
    Interceptor.attach(ptr(0x448D80), {
        onEnter: function (args) {
            const pIn = args[0];
            const pOut = args[1];
            const pLen = args[2].toInt32();

            console.log(`:: NyxLauncher::Md5(pIn ${pIn}, pOut ${pOut}, pLen ${pLen})`);
            if (pLen < 0x32) {
                console.log(hexdump(pIn, { length: pLen }));
            }

            this.pOut = pOut;
        },
        onLeave: function (retval) {
            console.log('Output');
            console.log(hexdump(this.pOut, { length: 0x20 }));
        }
    });

    // aes_encrypt
    Interceptor.attach(ptr(0x44E1B0), {
        onEnter: function (args) {
            const pIn = args[0];
            const pOut = args[1];
            const pCtx = args[2];

            console.log(`:: NyxLauncher::AesEncrypt(pCtx ${pIn}, pUk1 ${pOut}, pUk2 ${pCtx})`);
            console.log(hexdump(pIn, { length: 16 }));
        }
    });

    // Unknown.
    // Interceptor.attach(ptr(0x401A80), {
    //     onEnter: function (args) {
    //         const context = this.context as Ia32CpuContext;
    //         const pDest = args[0];
    //         const pData = context.ecx;
    //         const pDataLen = context.edx;

    //         console.log(`:: NyxLauncher::Unknown(${pDest}, ${pData}, ${pDataLen})`);
    //         // console.log('Input');
    //         // console.log(hexdump(pData, { length: pDataLen }));

    //         this.pDest = pDest;
    //     },
    //     onLeave: function (retval) {
    //         if (this.pDest.isNull()) {
    //             return;
    //         }
            
    //         console.log('Output');
    //         console.log(hexdump(this.pDest, { length: 0x20 }));
    //     }
    // })

    // Login packet encryption.
    Interceptor.attach(ptr(0x447DC0), {
        onEnter: function (args) {
            const pUnknown = args[0];
            const pStatic = args[1];
            const pUk2 = args[2].readCString();
            const pUk3 = args[3].readCString();
            const pRand = args[4].toInt32();
            const pBuf = args[5];
            const pUk6 = args[6];

            console.log(`:: NyxLauncher::EncryptLoginPacket(${pUnknown}, ${pStatic}, ${pUk2}, ${pUk3}, ${pRand}, ${pBuf}, ${pUk6})`);
            console.log(hexdump(pBuf, { length: 0x29 }));
        }
    });

    // Interceptor.attach(ptr(0x40FF2C), {
    //     onEnter: function (args) {
    //         const context = this.context as Ia32CpuContext;

    //         console.log(`:: NyxLauncher edx = ${context.edx.readPointer()}`);	
    //     }
    // });

    // Interceptor.attach(ptr(0x40EF9F), {
    //     onEnter: function (args) {
    //         const context = this.context as Ia32CpuContext;

    //         console.log(`:: NyxLauncher eax = ${context.eax.readPointer()}`);	
    //     }
    // });

    // ProcessStream(this, ptr, ptr, buffer, bufLen) 0x40F180
    // Interceptor.attach(ptr(0x40F180), {
    //     onEnter: function (args) {
    //         const pThis = args[0];
    //         const pPtr1 = args[1];
    //         const pPtr2 = args[2];
    //         const pBuffer = args[3];
    //         const pBufLen = args[4].toInt32();

    //         console.log(`:: NyxLauncher::ProcessStream(${pThis}, ${pPtr1}, ${pPtr2}, ${pBuffer}, ${pBufLen})`);
    //         console.log(hexdump(pBuffer, { length: pBufLen }));
    //     }
    // });

    // DoProcessStream(this, buffer, bufLen) 0x40EF40
    Interceptor.attach(ptr(0x40EF40), {
        onEnter: function (args) {
            const pThis = args[0];
            const pBuffer = args[1];
            const pBufLen = args[2];

            console.log(`:: NyxLauncher::DoProcessStream(${pThis}, ${pBuffer}, ${pBufLen})`);
            // console.log(hexdump(pBuffer, { length: pBufLen }));
        }
    });

    // OnMsg(this, ptr1, ptr2) 0x40EE90
    Interceptor.attach(ptr(0x40EE90), {
        onEnter: function (args) {
            const pThis = args[0];
            const pPtr1 = args[1];
            const pPtr2 = args[2];

            console.log(`:: NyxLauncher::OnMsg(${pThis}, ${pPtr1}, ${pPtr2})`);
        }
    });

    // Test 0x4041A0
    Interceptor.attach(ptr(0x4041A0), {
        onEnter: function (args) {
            console.log(`:: NyxLauncher 0x4041A0`);
        }
    });
    
    Interceptor.attach(ptr(0x401DF0), {
        onEnter: function (args) {
            console.log(`:: NyxLauncher 0x401DF0`);
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
}

waitForUnpack();