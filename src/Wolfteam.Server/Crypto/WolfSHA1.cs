// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace Wolfteam.Server.Crypto;

public static class WolfSHA1
{
    private const int HASH_LBLOCK = 16;
    private const int SHA_DIGEST_LENGTH = 20;

    private static readonly byte[] SHA1_END = [0x80,0x00,0x00,0x00];

    public static void Hash(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (output.Length != SHA_DIGEST_LENGTH)
        {
            throw new ArgumentException("Output length must be 20 bytes", nameof(output));
        }
        
        var ctx = new SHA_CTX
        {
            data = stackalloc uint[16]
        };

        unsafe
        {
            SHA1_Init(&ctx);
            SHA1_Update(&ctx, input, (uint)input.Length);
            SHA1_Final(&ctx);
        }
        
        BinaryPrimitives.WriteUInt32LittleEndian(output, ctx.h0);
        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(4), ctx.h1);
        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(8), ctx.h2);
        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(12), ctx.h3);
        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(16), ctx.h4);
    }

    private static unsafe void SHA1_Init(SHA_CTX* pCtx)
    {
        pCtx->h0 = 0x67452301;
        pCtx->h1 = 0xEFCDAB89;
        pCtx->h2 = 0x98BADCFE;
        pCtx->h3 = 0x10325476;
        pCtx->h4 = 0xC3D2E1F0;
        pCtx->Nl = 0;
        pCtx->Nh = 0;
        pCtx->num = 0;
    }

    private static unsafe void SHA1_Update(SHA_CTX* pCtx, ReadOnlySpan<byte> input, uint pLen)
    {
        if (pLen == 0) return;

        fixed (byte* pInputPtr = input)
        fixed (uint* pData = pCtx->data)
        {
            byte* pInput = pInputPtr;
            uint Nl = pCtx->Nl;
            uint v6 = Nl + (8 * pLen);
            if (v6 < Nl)
                pCtx->Nh++;
            
            int num = pCtx->num;
            pCtx->Nh += pLen >> 29;
            pCtx->Nl = v6;

            if (num != 0)
            {
                int numIndex = num >> 2;
                int numOffset = num & 3;
                uint* data = pData;
                uint temp = 0;

                if (pLen + num < 64)
                {
                    pCtx->num += (int)pLen;
                    temp = data[numIndex];

                    for (int i = 0; i < pLen; i++)
                    {
                        temp |= (uint)(*pInput++) << ((3 - numOffset) * 8);
                        numOffset++;
                        if (numOffset == 4)
                        {
                            data[numIndex++] = temp;
                            temp = 0;
                            numOffset = 0;
                        }
                    }

                    if (numOffset != 0)
                    {
                        data[numIndex] = temp;
                    }
                    return;
                }

                for (int i = 0; i < 64 - num; i++)
                {
                    temp |= (uint)(*pInput++) << ((3 - numOffset) * 8);
                    numOffset++;
                    if (numOffset == 4)
                    {
                        data[numIndex++] = temp;
                        temp = 0;
                        numOffset = 0;
                    }
                }

                pCtx->num = 0;
                HASH_BLOCK_HOST_ORDER(pCtx, pData, 1);
                pLen -= (uint)(64 - num);
            }

            if (pLen >= 64)
            {
                uint blocks = pLen >> 6;
                HASH_BLOCK_HOST_ORDER(pCtx, (uint*)pInput, blocks);
                pInput += blocks << 6;
                pLen &= 0x3F;
            }

            if (pLen > 0)
            {
                pCtx->num = (int)pLen;
                uint* data = pData;
                uint temp = 0;

                for (int i = 0; i < pLen; i++)
                {
                    temp |= (uint)(*pInput++) << ((3 - (i & 3)) * 8);
                    if ((i & 3) == 3)
                    {
                        *data++ = temp;
                        temp = 0;
                    }
                }

                if ((pLen & 3) != 0)
                {
                    *data = temp;
                }
            }
        }
    }

    private static unsafe void SHA1_Final(SHA_CTX* c)
    {
        fixed (uint* pData = c->data)
        fixed (byte* pEnd = SHA1_END)
        {
            uint *p;
            uint l;
            int i,j;

            /* c->num should definitly have room for at least one more byte. */
            p=pData;
            i=c->num>>2;
            j=c->num&0x03;

            l = (j == 0) ? 0u : pData[i];
            l = HOST_p_c2l(pEnd, l, ref j);
            p[i++]=l;

            if (i>(HASH_LBLOCK-2)) /* save room for Nl and Nh */
            {
                if (i<HASH_LBLOCK) p[i]=0;
                HASH_BLOCK_HOST_ORDER (c,p,1);
                i=0;
            }
            for (; i<(HASH_LBLOCK-2); i++)
                p[i]=0;

            p[HASH_LBLOCK-2]=c->Nh;
            p[HASH_LBLOCK-1]=c->Nl;
            HASH_BLOCK_HOST_ORDER (c,p,1);

            c->num=0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint HOST_p_c2l(byte* c, uint l, ref int n)
    {
        switch (n)
        {
            case 0:
                l = (uint)*c++ << 24;
                n++;
                goto case 1;
            case 1:
                l |= (uint)*c++ << 16;
                n++;
                goto case 2;
            case 2:
                l |= (uint)*c++ << 8;
                n++;
                goto case 3;
            case 3:
                l |= *c;
                break;
        }
        return l;
    }

    private static unsafe void HASH_BLOCK_HOST_ORDER(SHA_CTX *pCtx, uint *a2, uint a3)
    {
        uint h0; // esi
        uint h2; // edi
        uint h1; // ecx
        uint v8; // ecx
        uint v9; // edi
        uint v10; // esi
        uint v11; // ebx
        uint v12; // ecx
        uint v13; // esi
        uint v14; // edi
        uint v15; // ecx
        uint v16; // esi
        uint v17; // edi
        uint v18; // ecx
        uint v19; // esi
        uint v20; // edi
        uint v21; // ebx
        uint v22; // ecx
        uint v23; // esi
        uint v24; // edx
        uint v25; // ebx
        uint v26; // edi
        uint v27; // esi
        uint v28; // ebp
        uint v29; // edi
        uint v30; // esi
        uint v31; // ecx
        uint v32; // ebp
        uint v33; // edi
        uint v34; // ebx
        uint v35; // esi
        uint v36; // edx
        uint v37; // esi
        uint v38; // ebx
        uint v39; // ebx
        uint v40; // edx
        uint v41; // esi
        uint v42; // ecx
        uint v43; // ebx
        uint v44; // edi
        uint v45; // edx
        uint v46; // ebx
        uint v47; // esi
        uint v48; // ebx
        uint v49; // edi
        uint v50; // ebx
        uint v51; // ebx
        uint v52; // ebx
        uint v53; // ebx
        uint v54; // ebx
        uint v55; // ebx
        uint v56; // ebx
        uint v57; // ebx
        uint v58; // ecx
        uint v59; // edi
        uint v60; // ebx
        uint v61; // ebx
        uint v62; // edx
        uint v63; // ebx
        uint v64; // esi
        uint v65; // ebp
        uint v66; // ebx
        uint v67; // ebx
        uint v68; // ebp
        uint v69; // ebx
        uint v70; // ebx
        uint v71; // ebx
        uint v72; // edi
        uint v73; // ebx
        uint v74; // ecx
        uint v75; // ebx
        uint v76; // ecx
        uint *v77; // ecx
        uint v78; // edi
        uint v79; // esi
        uint v80; // edi
        uint v81; // edx
        uint v82; // esi
        uint v83; // edi
        uint v84; // edx
        uint v85; // esi
        uint v86; // edi
        uint v87; // edx
        uint v88; // esi
        uint v89; // edi
        uint v90; // edx
        uint v91; // esi
        uint v92; // edi
        uint v93; // ebx
        uint v94; // edx
        uint v95; // ebp
        uint v96; // ebx
        uint v97; // esi
        uint v98; // ebx
        uint v99; // ebx
        uint v100; // ebx
        uint v101; // ebx
        uint v102; // ebx
        uint v103; // ebx
        uint v104; // ebx
        uint v105; // ebx
        uint v106; // edi
        uint v107; // ebx
        uint v108; // esi
        uint v109; // esi
        uint v110; // ebp
        uint v111; // esi
        uint v112; // ebx
        uint v113; // ebx
        uint v114; // ebx
        uint v115; // esi
        uint v116; // ebx
        uint v117; // edi
        uint v118; // ebp
        uint v119; // edx
        uint v120; // ebx
        uint v121; // edi
        uint v122; // ebx
        uint v123; // ebx
        uint v124; // ebx
        uint v125; // ebx
        uint v126; // ebx
        uint v127; // ebx
        uint v128; // ebx
        uint v129; // edi
        uint v130; // esi
        uint v131; // ebx
        uint v132; // ebx
        uint v133; // edx
        uint v134; // ebx
        uint v135; // ebx
        uint v136; // ebx
        uint v137; // ebp
        uint v138; // ebx
        uint v139; // ebx
        uint v140; // ebx
        uint v141; // ebx
        uint v142; // edi
        uint v143; // ebx
        bool v144; // zf
        uint v145; // [esp+10h] [ebp-60h]
        uint v146; // [esp+10h] [ebp-60h]
        uint v147; // [esp+10h] [ebp-60h]
        uint v148; // [esp+10h] [ebp-60h]
        uint v149; // [esp+10h] [ebp-60h]
        uint v150; // [esp+10h] [ebp-60h]
        uint v151; // [esp+10h] [ebp-60h]
        uint v152; // [esp+10h] [ebp-60h]
        uint v153; // [esp+10h] [ebp-60h]
        uint v154; // [esp+10h] [ebp-60h]
        uint v155; // [esp+10h] [ebp-60h]
        uint v156; // [esp+10h] [ebp-60h]
        uint v157; // [esp+10h] [ebp-60h]
        uint v158; // [esp+10h] [ebp-60h]
        uint v159; // [esp+10h] [ebp-60h]
        uint v160; // [esp+10h] [ebp-60h]
        uint v161; // [esp+10h] [ebp-60h]
        uint v162; // [esp+10h] [ebp-60h]
        uint v163; // [esp+10h] [ebp-60h]
        uint v164; // [esp+10h] [ebp-60h]
        uint v165; // [esp+10h] [ebp-60h]
        uint v166; // [esp+10h] [ebp-60h]
        uint v167; // [esp+10h] [ebp-60h]
        uint v168; // [esp+10h] [ebp-60h]
        uint v169; // [esp+10h] [ebp-60h]
        uint v170; // [esp+10h] [ebp-60h]
        uint v171; // [esp+10h] [ebp-60h]
        uint v172; // [esp+10h] [ebp-60h]
        uint v173; // [esp+10h] [ebp-60h]
        uint v174; // [esp+10h] [ebp-60h]
        uint v175; // [esp+10h] [ebp-60h]
        uint v176; // [esp+10h] [ebp-60h]
        uint v177; // [esp+10h] [ebp-60h]
        uint v178; // [esp+10h] [ebp-60h]
        uint v179; // [esp+10h] [ebp-60h]
        uint v180; // [esp+10h] [ebp-60h]
        uint v181; // [esp+10h] [ebp-60h]
        uint v182; // [esp+10h] [ebp-60h]
        uint v183; // [esp+10h] [ebp-60h]
        uint v184; // [esp+10h] [ebp-60h]
        uint v185; // [esp+10h] [ebp-60h]
        uint v186; // [esp+14h] [ebp-5Ch]
        uint v187; // [esp+14h] [ebp-5Ch]
        uint v188; // [esp+14h] [ebp-5Ch]
        uint v189; // [esp+14h] [ebp-5Ch]
        uint v190; // [esp+14h] [ebp-5Ch]
        uint v191; // [esp+14h] [ebp-5Ch]
        uint v192; // [esp+14h] [ebp-5Ch]
        uint v193; // [esp+14h] [ebp-5Ch]
        uint v194; // [esp+14h] [ebp-5Ch]
        uint v195; // [esp+14h] [ebp-5Ch]
        uint v196; // [esp+14h] [ebp-5Ch]
        uint v197; // [esp+14h] [ebp-5Ch]
        uint v198; // [esp+14h] [ebp-5Ch]
        uint v199; // [esp+14h] [ebp-5Ch]
        uint v200; // [esp+14h] [ebp-5Ch]
        uint v201; // [esp+14h] [ebp-5Ch]
        uint v202; // [esp+14h] [ebp-5Ch]
        uint v203; // [esp+14h] [ebp-5Ch]
        uint v204; // [esp+14h] [ebp-5Ch]
        uint v205; // [esp+14h] [ebp-5Ch]
        uint v206; // [esp+14h] [ebp-5Ch]
        uint v207; // [esp+14h] [ebp-5Ch]
        uint v208; // [esp+14h] [ebp-5Ch]
        uint v209; // [esp+14h] [ebp-5Ch]
        uint v210; // [esp+14h] [ebp-5Ch]
        uint v211; // [esp+14h] [ebp-5Ch]
        uint v212; // [esp+14h] [ebp-5Ch]
        uint v213; // [esp+14h] [ebp-5Ch]
        uint v214; // [esp+14h] [ebp-5Ch]
        uint v215; // [esp+14h] [ebp-5Ch]
        uint v216; // [esp+14h] [ebp-5Ch]
        uint v217; // [esp+14h] [ebp-5Ch]
        uint v218; // [esp+14h] [ebp-5Ch]
        uint v219; // [esp+14h] [ebp-5Ch]
        uint v220; // [esp+14h] [ebp-5Ch]
        uint v221; // [esp+14h] [ebp-5Ch]
        uint v222; // [esp+14h] [ebp-5Ch]
        uint v223; // [esp+18h] [ebp-58h]
        uint v224; // [esp+18h] [ebp-58h]
        uint v225; // [esp+18h] [ebp-58h]
        uint v226; // [esp+18h] [ebp-58h]
        uint v227; // [esp+18h] [ebp-58h]
        uint v228; // [esp+18h] [ebp-58h]
        uint v229; // [esp+18h] [ebp-58h]
        uint v230; // [esp+18h] [ebp-58h]
        uint v231; // [esp+18h] [ebp-58h]
        uint v232; // [esp+18h] [ebp-58h]
        uint v233; // [esp+18h] [ebp-58h]
        uint v234; // [esp+18h] [ebp-58h]
        uint v235; // [esp+18h] [ebp-58h]
        uint v236; // [esp+18h] [ebp-58h]
        uint v237; // [esp+18h] [ebp-58h]
        uint v238; // [esp+18h] [ebp-58h]
        uint v239; // [esp+18h] [ebp-58h]
        uint v240; // [esp+18h] [ebp-58h]
        uint v241; // [esp+18h] [ebp-58h]
        uint v242; // [esp+18h] [ebp-58h]
        uint v243; // [esp+18h] [ebp-58h]
        uint v244; // [esp+18h] [ebp-58h]
        uint v245; // [esp+18h] [ebp-58h]
        uint v246; // [esp+18h] [ebp-58h]
        uint v247; // [esp+18h] [ebp-58h]
        uint v248; // [esp+18h] [ebp-58h]
        uint v249; // [esp+18h] [ebp-58h]
        uint v250; // [esp+18h] [ebp-58h]
        uint v251; // [esp+18h] [ebp-58h]
        uint v252; // [esp+18h] [ebp-58h]
        uint v253; // [esp+18h] [ebp-58h]
        uint v254; // [esp+18h] [ebp-58h]
        uint v255; // [esp+18h] [ebp-58h]
        uint v256; // [esp+18h] [ebp-58h]
        uint v257; // [esp+18h] [ebp-58h]
        uint v258; // [esp+18h] [ebp-58h]
        uint v259; // [esp+18h] [ebp-58h]
        uint v260; // [esp+18h] [ebp-58h]
        uint v261; // [esp+18h] [ebp-58h]
        uint v262; // [esp+18h] [ebp-58h]
        uint v263; // [esp+18h] [ebp-58h]
        uint v264; // [esp+18h] [ebp-58h]
        uint v265; // [esp+18h] [ebp-58h]
        uint v266; // [esp+18h] [ebp-58h]
        uint v267; // [esp+1Ch] [ebp-54h]
        uint v268; // [esp+1Ch] [ebp-54h]
        uint v269; // [esp+1Ch] [ebp-54h]
        uint v270; // [esp+1Ch] [ebp-54h]
        uint v271; // [esp+1Ch] [ebp-54h]
        uint v272; // [esp+1Ch] [ebp-54h]
        uint v273; // [esp+1Ch] [ebp-54h]
        uint v274; // [esp+1Ch] [ebp-54h]
        uint v275; // [esp+1Ch] [ebp-54h]
        uint v276; // [esp+1Ch] [ebp-54h]
        uint v277; // [esp+1Ch] [ebp-54h]
        uint v278; // [esp+1Ch] [ebp-54h]
        uint v279; // [esp+1Ch] [ebp-54h]
        uint v280; // [esp+1Ch] [ebp-54h]
        uint v281; // [esp+1Ch] [ebp-54h]
        uint v282; // [esp+1Ch] [ebp-54h]
        uint v283; // [esp+1Ch] [ebp-54h]
        uint v284; // [esp+1Ch] [ebp-54h]
        uint v285; // [esp+1Ch] [ebp-54h]
        uint v286; // [esp+1Ch] [ebp-54h]
        uint v287; // [esp+1Ch] [ebp-54h]
        uint v288; // [esp+1Ch] [ebp-54h]
        uint v289; // [esp+1Ch] [ebp-54h]
        uint v290; // [esp+1Ch] [ebp-54h]
        uint v291; // [esp+1Ch] [ebp-54h]
        uint v292; // [esp+1Ch] [ebp-54h]
        uint v293; // [esp+1Ch] [ebp-54h]
        uint v294; // [esp+1Ch] [ebp-54h]
        uint v295; // [esp+1Ch] [ebp-54h]
        uint v296; // [esp+1Ch] [ebp-54h]
        uint v297; // [esp+1Ch] [ebp-54h]
        uint v298; // [esp+1Ch] [ebp-54h]
        uint v299; // [esp+1Ch] [ebp-54h]
        uint v300; // [esp+1Ch] [ebp-54h]
        uint v301; // [esp+1Ch] [ebp-54h]
        uint v302; // [esp+1Ch] [ebp-54h]
        uint v303; // [esp+1Ch] [ebp-54h]
        uint v304; // [esp+1Ch] [ebp-54h]
        uint v305; // [esp+1Ch] [ebp-54h]
        uint v306; // [esp+1Ch] [ebp-54h]
        uint v307; // [esp+20h] [ebp-50h]
        uint v308; // [esp+20h] [ebp-50h]
        uint v309; // [esp+20h] [ebp-50h]
        uint v310; // [esp+20h] [ebp-50h]
        uint v311; // [esp+20h] [ebp-50h]
        uint v312; // [esp+20h] [ebp-50h]
        uint v313; // [esp+20h] [ebp-50h]
        uint v314; // [esp+20h] [ebp-50h]
        uint v315; // [esp+24h] [ebp-4Ch]
        uint v316; // [esp+24h] [ebp-4Ch]
        uint v317; // [esp+24h] [ebp-4Ch]
        uint v318; // [esp+24h] [ebp-4Ch]
        uint v319; // [esp+24h] [ebp-4Ch]
        uint v320; // [esp+24h] [ebp-4Ch]
        uint v321; // [esp+24h] [ebp-4Ch]
        uint v322; // [esp+24h] [ebp-4Ch]
        uint v323; // [esp+28h] [ebp-48h]
        uint v324; // [esp+28h] [ebp-48h]
        uint v325; // [esp+28h] [ebp-48h]
        uint v326; // [esp+28h] [ebp-48h]
        uint v327; // [esp+28h] [ebp-48h]
        uint v328; // [esp+28h] [ebp-48h]
        uint v329; // [esp+28h] [ebp-48h]
        uint v330; // [esp+28h] [ebp-48h]
        uint v331; // [esp+2Ch] [ebp-44h]
        uint v332; // [esp+2Ch] [ebp-44h]
        uint v333; // [esp+2Ch] [ebp-44h]
        uint v334; // [esp+2Ch] [ebp-44h]
        uint v335; // [esp+2Ch] [ebp-44h]
        uint v336; // [esp+2Ch] [ebp-44h]
        uint v337; // [esp+2Ch] [ebp-44h]
        uint v338; // [esp+2Ch] [ebp-44h]
        uint v339; // [esp+30h] [ebp-40h]
        uint v340; // [esp+30h] [ebp-40h]
        uint v341; // [esp+30h] [ebp-40h]
        uint v342; // [esp+30h] [ebp-40h]
        uint v343; // [esp+30h] [ebp-40h]
        uint v344; // [esp+30h] [ebp-40h]
        uint v345; // [esp+30h] [ebp-40h]
        uint v346; // [esp+30h] [ebp-40h]
        uint v347; // [esp+30h] [ebp-40h]
        uint v348; // [esp+34h] [ebp-3Ch]
        uint v349; // [esp+34h] [ebp-3Ch]
        uint v350; // [esp+34h] [ebp-3Ch]
        uint v351; // [esp+34h] [ebp-3Ch]
        uint v352; // [esp+34h] [ebp-3Ch]
        uint v353; // [esp+34h] [ebp-3Ch]
        uint v354; // [esp+34h] [ebp-3Ch]
        uint v355; // [esp+34h] [ebp-3Ch]
        uint v356; // [esp+34h] [ebp-3Ch]
        uint v357; // [esp+34h] [ebp-3Ch]
        uint v358; // [esp+38h] [ebp-38h]
        uint v359; // [esp+38h] [ebp-38h]
        uint v360; // [esp+38h] [ebp-38h]
        uint v361; // [esp+38h] [ebp-38h]
        uint v362; // [esp+38h] [ebp-38h]
        uint v363; // [esp+38h] [ebp-38h]
        uint v364; // [esp+38h] [ebp-38h]
        uint v365; // [esp+38h] [ebp-38h]
        uint v366; // [esp+38h] [ebp-38h]
        uint v367; // [esp+38h] [ebp-38h]
        uint v368; // [esp+3Ch] [ebp-34h]
        uint v369; // [esp+3Ch] [ebp-34h]
        uint v370; // [esp+3Ch] [ebp-34h]
        uint v371; // [esp+3Ch] [ebp-34h]
        uint v372; // [esp+3Ch] [ebp-34h]
        uint v373; // [esp+3Ch] [ebp-34h]
        uint v374; // [esp+3Ch] [ebp-34h]
        uint v375; // [esp+3Ch] [ebp-34h]
        uint v376; // [esp+3Ch] [ebp-34h]
        uint v377; // [esp+3Ch] [ebp-34h]
        uint v378; // [esp+40h] [ebp-30h]
        uint v379; // [esp+40h] [ebp-30h]
        uint v380; // [esp+40h] [ebp-30h]
        uint v381; // [esp+40h] [ebp-30h]
        uint v382; // [esp+40h] [ebp-30h]
        uint v383; // [esp+40h] [ebp-30h]
        uint v384; // [esp+40h] [ebp-30h]
        uint v385; // [esp+40h] [ebp-30h]
        uint v386; // [esp+40h] [ebp-30h]
        uint v387; // [esp+40h] [ebp-30h]
        uint v388; // [esp+44h] [ebp-2Ch]
        uint v389; // [esp+44h] [ebp-2Ch]
        uint v390; // [esp+44h] [ebp-2Ch]
        uint v391; // [esp+44h] [ebp-2Ch]
        uint v392; // [esp+44h] [ebp-2Ch]
        uint v393; // [esp+44h] [ebp-2Ch]
        uint v394; // [esp+44h] [ebp-2Ch]
        uint v395; // [esp+44h] [ebp-2Ch]
        uint v396; // [esp+44h] [ebp-2Ch]
        uint v397; // [esp+44h] [ebp-2Ch]
        uint v398; // [esp+48h] [ebp-28h]
        uint v399; // [esp+48h] [ebp-28h]
        uint v400; // [esp+48h] [ebp-28h]
        uint v401; // [esp+48h] [ebp-28h]
        uint v402; // [esp+48h] [ebp-28h]
        uint v403; // [esp+48h] [ebp-28h]
        uint v404; // [esp+48h] [ebp-28h]
        uint v405; // [esp+48h] [ebp-28h]
        uint v406; // [esp+4Ch] [ebp-24h]
        uint v407; // [esp+4Ch] [ebp-24h]
        uint v408; // [esp+4Ch] [ebp-24h]
        uint v409; // [esp+4Ch] [ebp-24h]
        uint v410; // [esp+4Ch] [ebp-24h]
        uint v411; // [esp+4Ch] [ebp-24h]
        uint v412; // [esp+4Ch] [ebp-24h]
        uint v413; // [esp+4Ch] [ebp-24h]
        uint v414; // [esp+50h] [ebp-20h]
        uint v415; // [esp+50h] [ebp-20h]
        uint v416; // [esp+50h] [ebp-20h]
        uint v417; // [esp+50h] [ebp-20h]
        uint v418; // [esp+50h] [ebp-20h]
        uint v419; // [esp+50h] [ebp-20h]
        uint v420; // [esp+50h] [ebp-20h]
        uint v421; // [esp+54h] [ebp-1Ch]
        uint v422; // [esp+54h] [ebp-1Ch]
        uint v423; // [esp+54h] [ebp-1Ch]
        uint v424; // [esp+54h] [ebp-1Ch]
        uint v425; // [esp+54h] [ebp-1Ch]
        uint v426; // [esp+54h] [ebp-1Ch]
        uint v427; // [esp+54h] [ebp-1Ch]
        uint v428; // [esp+54h] [ebp-1Ch]
        uint v429; // [esp+58h] [ebp-18h]
        uint v430; // [esp+58h] [ebp-18h]
        uint v431; // [esp+58h] [ebp-18h]
        uint v432; // [esp+58h] [ebp-18h]
        uint v433; // [esp+58h] [ebp-18h]
        uint v434; // [esp+58h] [ebp-18h]
        uint v435; // [esp+5Ch] [ebp-14h]
        uint v436; // [esp+5Ch] [ebp-14h]
        uint v437; // [esp+5Ch] [ebp-14h]
        uint v438; // [esp+5Ch] [ebp-14h]
        uint v439; // [esp+5Ch] [ebp-14h]
        uint v440; // [esp+5Ch] [ebp-14h]
        uint v441; // [esp+60h] [ebp-10h]
        uint v442; // [esp+60h] [ebp-10h]
        uint v443; // [esp+64h] [ebp-Ch]
        uint v444; // [esp+68h] [ebp-8h]
        uint *v445; // [esp+6Ch] [ebp-4h]
        uint v446; // [esp+74h] [ebp+4h]
        uint v447; // [esp+74h] [ebp+4h]
        uint v448; // [esp+74h] [ebp+4h]
        uint v449; // [esp+74h] [ebp+4h]
        uint v450; // [esp+74h] [ebp+4h]
        uint v451; // [esp+74h] [ebp+4h]
        uint v452; // [esp+74h] [ebp+4h]
        uint v453; // [esp+74h] [ebp+4h]
        uint v454; // [esp+74h] [ebp+4h]
        uint v455; // [esp+74h] [ebp+4h]
        uint v456; // [esp+74h] [ebp+4h]
        uint v457; // [esp+74h] [ebp+4h]
        uint v458; // [esp+74h] [ebp+4h]
        uint v459; // [esp+74h] [ebp+4h]
        uint v460; // [esp+74h] [ebp+4h]
        uint v461; // [esp+74h] [ebp+4h]
        uint v462; // [esp+74h] [ebp+4h]
        uint v463; // [esp+74h] [ebp+4h]
        uint v464; // [esp+74h] [ebp+4h]
        uint v465; // [esp+74h] [ebp+4h]
        uint v466; // [esp+74h] [ebp+4h]
        uint v467; // [esp+74h] [ebp+4h]
        uint v468; // [esp+74h] [ebp+4h]
        uint v469; // [esp+74h] [ebp+4h]
        uint v470; // [esp+74h] [ebp+4h]
        uint v471; // [esp+74h] [ebp+4h]
        uint v472; // [esp+74h] [ebp+4h]
        uint v473; // [esp+74h] [ebp+4h]
        uint v474; // [esp+74h] [ebp+4h]
        uint v475; // [esp+74h] [ebp+4h]
        uint v476; // [esp+74h] [ebp+4h]
        uint v477; // [esp+74h] [ebp+4h]
        uint v478; // [esp+74h] [ebp+4h]
        uint v479; // [esp+74h] [ebp+4h]
        uint v480; // [esp+74h] [ebp+4h]
        uint v481; // [esp+74h] [ebp+4h]
        uint v482; // [esp+74h] [ebp+4h]
        uint v483; // [esp+74h] [ebp+4h]
        uint v484; // [esp+74h] [ebp+4h]
        uint v485; // [esp+74h] [ebp+4h]
        uint v486; // [esp+74h] [ebp+4h]
        uint v487; // [esp+74h] [ebp+4h]
        uint v488; // [esp+74h] [ebp+4h]
        uint v489; // [esp+74h] [ebp+4h]
        uint v490; // [esp+78h] [ebp+8h]
        uint v491; // [esp+78h] [ebp+8h]
        uint v492; // [esp+78h] [ebp+8h]
        uint v493; // [esp+78h] [ebp+8h]
        uint v494; // [esp+78h] [ebp+8h]
        uint v495; // [esp+78h] [ebp+8h]
        uint v496; // [esp+78h] [ebp+8h]
        uint v497; // [esp+78h] [ebp+8h]
        uint v498; // [esp+78h] [ebp+8h]
        uint v499; // [esp+78h] [ebp+8h]
        uint v500; // [esp+78h] [ebp+8h]
        uint v501; // [esp+78h] [ebp+8h]
        uint v502; // [esp+78h] [ebp+8h]
        uint v503; // [esp+78h] [ebp+8h]
        uint v504; // [esp+78h] [ebp+8h]
        uint v505; // [esp+78h] [ebp+8h]
        uint v506; // [esp+78h] [ebp+8h]
        uint v507; // [esp+78h] [ebp+8h]
        uint v508; // [esp+78h] [ebp+8h]
        uint v509; // [esp+78h] [ebp+8h]
        uint v510; // [esp+78h] [ebp+8h]
        uint v511; // [esp+78h] [ebp+8h]
        uint v512; // [esp+78h] [ebp+8h]
        uint v513; // [esp+78h] [ebp+8h]
        uint v514; // [esp+78h] [ebp+8h]
        uint v515; // [esp+78h] [ebp+8h]
        uint v516; // [esp+78h] [ebp+8h]
        uint v517; // [esp+78h] [ebp+8h]
        uint v518; // [esp+78h] [ebp+8h]
        uint v519; // [esp+78h] [ebp+8h]
        uint v520; // [esp+78h] [ebp+8h]
        uint v521; // [esp+78h] [ebp+8h]
        uint v522; // [esp+78h] [ebp+8h]
        uint v523; // [esp+78h] [ebp+8h]
        uint v524; // [esp+78h] [ebp+8h]
        uint v525; // [esp+78h] [ebp+8h]
        uint v526; // [esp+78h] [ebp+8h]
        uint v527; // [esp+78h] [ebp+8h]
        uint v528; // [esp+78h] [ebp+8h]
        uint v529; // [esp+78h] [ebp+8h]
        uint v530; // [esp+7Ch] [ebp+Ch]

        v435 = *a2;
        h0 = pCtx->h0;
        h2 = pCtx->h2;
        h1 = pCtx->h1;
        v406 = a2[1];
        v446 = pCtx->h4 + BitOperations.RotateLeft(pCtx->h0, 5) + (pCtx->h3 ^ h1 & (h2 ^ pCtx->h3)) + *a2 + 1518500249;
        v8 = BitOperations.RotateLeft(h1, 30);
        v9 = pCtx->h3 + v406 + BitOperations.RotateLeft(v446, 5) + (h2 ^ h0 & (v8 ^ h2)) + 1518500249;
        v10 = BitOperations.RotateLeft(h0, 30);
        v445 = a2 + 2;
        v378 = a2[2];
        v11 = v378 + BitOperations.RotateLeft(v9, 5) + (v8 ^ v446 & (v10 ^ v8)) + pCtx->h2 + 1518500249;
        v447 = BitOperations.RotateLeft(v446, 30);
        v339 = a2[3];
        v12 = v8 + v339 + BitOperations.RotateLeft(v11, 5) + (v10 ^ v9 & (v10 ^ v447)) + 1518500249;
        v368 = a2[4];
        v490 = BitOperations.RotateLeft(v9, 30);
        v13 = v10 + v368 + BitOperations.RotateLeft(v12, 5) + (v447 ^ v11 & (v490 ^ v447)) + 1518500249;
        v358 = a2[5];
        v186 = BitOperations.RotateLeft(v11, 30);
        v14 = v447 + v358 + BitOperations.RotateLeft(v13, 5) + (v490 ^ v12 & (v186 ^ v490)) + 1518500249;
        v348 = a2[6];
        v223 = BitOperations.RotateLeft(v12, 30);
        v15 = v490 + v348 + BitOperations.RotateLeft(v14, 5) + (v186 ^ v13 & (v223 ^ v186)) + 1518500249;
        v443 = a2[7];
        v267 = BitOperations.RotateLeft(v13, 30);
        v16 = v186 + v443 + BitOperations.RotateLeft(v15, 5) + (v223 ^ v14 & (v267 ^ v223)) + 1518500249;
        v421 = a2[8];
        v145 = BitOperations.RotateLeft(v14, 30);
        v17 = v223 + v421 + BitOperations.RotateLeft(v16, 5) + (v267 ^ v15 & (v145 ^ v267)) + 1518500249;
        v448 = BitOperations.RotateLeft(v15, 30);
        v414 = a2[9];
        v18 = v267 + v414 + BitOperations.RotateLeft(v17, 5) + (v145 ^ v16 & (v145 ^ v448)) + 1518500249;
        v491 = BitOperations.RotateLeft(v16, 30);
        v398 = a2[10];
        v19 = v145 + v398 + BitOperations.RotateLeft(v18, 5) + (v448 ^ v17 & (v491 ^ v448)) + 1518500249;
        v187 = BitOperations.RotateLeft(v17, 30);
        v388 = a2[11];
        v20 = v448 + v388 + BitOperations.RotateLeft(v19, 5) + (v491 ^ v18 & (v187 ^ v491)) + 1518500249;
        v224 = BitOperations.RotateLeft(v18, 30);
        v441 = a2[12];
        v21 = v491 + v441 + BitOperations.RotateLeft(v20, 5) + (v187 ^ v19 & (v224 ^ v187)) + 1518500249;
        v268 = BitOperations.RotateLeft(v19, 30);
        v429 = a2[13];
        v22 = a2[14];
        v492 = v187 + v429 + BitOperations.RotateLeft(v21, 5) + (v224 ^ v20 & (v268 ^ v224)) + 1518500249;
        v146 = BitOperations.RotateLeft(v20, 30);
        v23 = v224 + v22 + BitOperations.RotateLeft(v492, 5) + (v268 ^ v21 & (v146 ^ v268)) + 1518500249;
        v24 = a2[15];
        v25 = BitOperations.RotateLeft(v21, 30);
        v26 = v268 + v24 + BitOperations.RotateLeft(v23, 5) + (v146 ^ v492 & (v146 ^ v25)) + 1518500249;
        v493 = BitOperations.RotateLeft(v492, 30);
        v436 = v435 ^ v378 ^ v421 ^ v429;
        v188 = BitOperations.RotateLeft(v23, 30);
        v269 = v436 + v146 + BitOperations.RotateLeft(v26, 5) + (v25 ^ v23 & (v493 ^ v25)) + 1518500249;
        v407 = v406 ^ v339 ^ v414 ^ v22;
        v225 = BitOperations.RotateLeft(v26, 30);
        v147 = v407 + v25 + BitOperations.RotateLeft(v269, 5) + (v493 ^ v26 & (v188 ^ v493)) + 1518500249;
        v307 = v378 ^ v368 ^ v398 ^ v24;
        v27 = v307 + v493 + BitOperations.RotateLeft(v147, 5) + (v188 ^ v269 & (v225 ^ v188)) + 1518500249;
        v270 = BitOperations.RotateLeft(v269, 30);
        v28 = v188 + BitOperations.RotateLeft(v27, 5) + (v225 ^ v147 & (v270 ^ v225));
        v148 = BitOperations.RotateLeft(v147, 30);
        v315 = v436 ^ v339 ^ v358 ^ v388;
        v494 = v315 + v28 + 1518500249;
        v323 = v407 ^ v368 ^ v348 ^ v441;
        v449 = BitOperations.RotateLeft(v27, 30);
        v189 = v323 + v225 + BitOperations.RotateLeft(v494, 5) + (v148 ^ v270 ^ v27) + 1859775393;
        v331 = v307 ^ v358 ^ v443 ^ v429;
        v226 = v331 + v270 + BitOperations.RotateLeft(v189, 5) + (v148 ^ v494 ^ v449) + 1859775393;
        v495 = BitOperations.RotateLeft(v494, 30);
        v340 = v315 ^ v348 ^ v421 ^ v22;
        v271 = v340 + v148 + BitOperations.RotateLeft(v226, 5) + (v189 ^ v495 ^ v449) + 1859775393;
        v190 = BitOperations.RotateLeft(v189, 30);
        v349 = v323 ^ v443 ^ v414 ^ v24;
        v149 = v349 + v449 + BitOperations.RotateLeft(v271, 5) + (v226 ^ v190 ^ v495) + 1859775393;
        v227 = BitOperations.RotateLeft(v226, 30);
        v359 = v436 ^ v331 ^ v421 ^ v398;
        v450 = v359 + v495 + BitOperations.RotateLeft(v149, 5) + (v271 ^ v227 ^ v190) + 1859775393;
        v272 = BitOperations.RotateLeft(v271, 30);
        v369 = v407 ^ v340 ^ v414 ^ v388;
        v29 = v369 + v190 + BitOperations.RotateLeft(v450, 5) + (v149 ^ v272 ^ v227) + 1859775393;
        v150 = BitOperations.RotateLeft(v149, 30);
        v379 = v307 ^ v349 ^ v398 ^ v441;
        v191 = v379 + v227 + BitOperations.RotateLeft(v29, 5) + (v150 ^ v272 ^ v450) + 1859775393;
        v451 = BitOperations.RotateLeft(v450, 30);
        v389 = v315 ^ v359 ^ v388 ^ v429;
        v228 = v389 + v272 + BitOperations.RotateLeft(v191, 5) + (v150 ^ v29 ^ v451) + 1859775393;
        v496 = BitOperations.RotateLeft(v29, 30);
        v422 = v323 ^ v369 ^ v441 ^ v22;
        v273 = v422 + v150 + BitOperations.RotateLeft(v228, 5) + (v191 ^ v496 ^ v451) + 1859775393;
        v192 = BitOperations.RotateLeft(v191, 30);
        v399 = v331 ^ v379 ^ v429 ^ v24;
        v30 = v399 + v451 + BitOperations.RotateLeft(v273, 5) + (v228 ^ v192 ^ v496) + 1859775393;
        v31 = v436 ^ v340 ^ v389 ^ v22;
        v229 = BitOperations.RotateLeft(v228, 30);
        v32 = BitOperations.RotateLeft(v273, 30);
        v33 = v407 ^ v349 ^ v422 ^ v24;
        v452 = v496 + BitOperations.RotateLeft(v30, 5) + (v273 ^ v229 ^ v192) + v31 + 1859775393;
        v151 = BitOperations.RotateLeft(v30, 30);
        v497 = v33 + v192 + BitOperations.RotateLeft(v452, 5) + (v30 ^ v32 ^ v229) + 1859775393;
        v437 = v436 ^ v307 ^ v359 ^ v399;
        v193 = v437 + v229 + BitOperations.RotateLeft(v497, 5) + (v151 ^ v32 ^ v452) + 1859775393;
        v453 = BitOperations.RotateLeft(v452, 30);
        v34 = v32 + BitOperations.RotateLeft(v193, 5) + (v151 ^ v497 ^ v453);
        v408 = v407 ^ v315 ^ v369 ^ v31;
        v498 = BitOperations.RotateLeft(v497, 30);
        v230 = v408 + v34 + 1859775393;
        v35 = BitOperations.RotateLeft(v193, 30);
        v308 = v307 ^ v323 ^ v379 ^ v33;
        v274 = v308 + v151 + BitOperations.RotateLeft(v230, 5) + (v193 ^ v498 ^ v453) + 1859775393;
        v316 = v437 ^ v315 ^ v331 ^ v389;
        v152 = v316 + v453 + BitOperations.RotateLeft(v274, 5) + (v230 ^ v35 ^ v498) + 1859775393;
        v231 = BitOperations.RotateLeft(v230, 30);
        v324 = v408 ^ v323 ^ v340 ^ v422;
        v454 = v324 + v498 + BitOperations.RotateLeft(v152, 5) + (v274 ^ v231 ^ v35) + 1859775393;
        v275 = BitOperations.RotateLeft(v274, 30);
        v332 = v308 ^ v331 ^ v349 ^ v399;
        v499 = v332 + v35 + BitOperations.RotateLeft(v454, 5) + (v152 ^ v275 ^ v231) + 1859775393;
        v153 = BitOperations.RotateLeft(v152, 30);
        v341 = v316 ^ v340 ^ v359 ^ v31;
        v36 = v341 + v231 + BitOperations.RotateLeft(v499, 5) + (v153 ^ v275 ^ v454) + 1859775393;
        v350 = v324 ^ v349 ^ v369 ^ v33;
        v455 = BitOperations.RotateLeft(v454, 30);
        v37 = BitOperations.RotateLeft(v499, 30);
        v232 = v350 + v275 + BitOperations.RotateLeft(v36, 5) + (v153 ^ v499 ^ v455) + 1859775393;
        v360 = v437 ^ v332 ^ v359 ^ v379;
        v370 = v408 ^ v341 ^ v369 ^ v389;
        v194 = BitOperations.RotateLeft(v36, 30);
        v276 = BitOperations.RotateLeft(v232, 5) + v360 + v153 + (v36 & v37 | v455 & (v36 | v37)) - 1894007588;
        v38 = BitOperations.RotateLeft(v276, 5) + v370 + v455 + (v232 & v194 | v37 & (v232 | v194)) - 1894007588;
        v380 = v308 ^ v350 ^ v379 ^ v422;
        v233 = BitOperations.RotateLeft(v232, 30);
        v154 = v38;
        v39 = BitOperations.RotateLeft(v38, 5) + v380 + v37 + (v276 & v233 | v194 & (v276 | v233)) - 1894007588;
        v390 = v316 ^ v360 ^ v389 ^ v399;
        v277 = BitOperations.RotateLeft(v276, 30);
        v423 = v324 ^ v370 ^ v422 ^ v31;
        v500 = BitOperations.RotateLeft(v39, 5) + v390 + v194 + (v154 & v277 | v233 & (v154 | v277)) - 1894007588;
        v155 = BitOperations.RotateLeft(v154, 30);
        v456 = BitOperations.RotateLeft(v39, 30);
        v195 = BitOperations.RotateLeft(v500, 5) + v423 + v233 + (v155 & v39 | v277 & (v155 | v39)) - 1894007588;
        v400 = v332 ^ v380 ^ v399 ^ v33;
        v40 = BitOperations.RotateLeft(v195, 5) + v400 + v277 + (v500 & v456 | v155 & (v500 | v456)) - 1894007588;
        v41 = BitOperations.RotateLeft(v500, 30);
        v430 = v437 ^ v341 ^ v390 ^ v31;
        v42 = v430 + v155 + (v195 & v41 | v456 & (v195 | v41)) + BitOperations.RotateLeft(v40, 5) - 1894007588;
        v43 = BitOperations.RotateLeft(v195, 30);
        v415 = v408 ^ v350 ^ v423 ^ v33;
        v44 = BitOperations.RotateLeft(v42, 5) + v415 + v456 + (v40 & v43 | v41 & (v40 | v43)) - 1894007588;
        v234 = BitOperations.RotateLeft(v40, 30);
        v45 = v437 ^ v308 ^ v360 ^ v400;
        v196 = v43;
        v46 = v45 + v41 + (v42 & v234 | v43 & (v42 | v234)) + BitOperations.RotateLeft(v44, 5) - 1894007588;
        v47 = v408 ^ v316 ^ v370 ^ v430;
        v278 = BitOperations.RotateLeft(v42, 30);
        v457 = v46;
        v48 = v47 + v196 + (v44 & v278 | v234 & (v44 | v278)) + BitOperations.RotateLeft(v46, 5) - 1894007588;
        v309 = v308 ^ v324 ^ v380 ^ v415;
        v49 = BitOperations.RotateLeft(v44, 30);
        v501 = v48;
        v50 = BitOperations.RotateLeft(v48, 5) + v309 + v234 + (v49 & v457 | v278 & (v49 | v457)) - 1894007588;
        v317 = v45 ^ v316 ^ v332 ^ v390;
        v458 = BitOperations.RotateLeft(v457, 30);
        v197 = v50;
        v51 = BitOperations.RotateLeft(v50, 5) + v317 + v278 + (v501 & v458 | v49 & (v501 | v458)) - 1894007588;
        v325 = v47 ^ v324 ^ v341 ^ v423;
        v502 = BitOperations.RotateLeft(v501, 30);
        v235 = v51;
        v52 = BitOperations.RotateLeft(v51, 5) + v325 + v49 + (v197 & v502 | v458 & (v197 | v502)) - 1894007588;
        v333 = v309 ^ v332 ^ v350 ^ v400;
        v198 = BitOperations.RotateLeft(v197, 30);
        v279 = v52;
        v53 = BitOperations.RotateLeft(v52, 5) + v333 + v458 + (v235 & v198 | v502 & (v235 | v198)) - 1894007588;
        v342 = v317 ^ v341 ^ v360 ^ v430;
        v236 = BitOperations.RotateLeft(v235, 30);
        v156 = v53;
        v54 = BitOperations.RotateLeft(v53, 5) + v342 + v502 + (v279 & v236 | v198 & (v279 | v236)) - 1894007588;
        v351 = v325 ^ v350 ^ v370 ^ v415;
        v280 = BitOperations.RotateLeft(v279, 30);
        v459 = v54;
        v55 = BitOperations.RotateLeft(v54, 5) + v351 + v198 + (v156 & v280 | v236 & (v156 | v280)) - 1894007588;
        v361 = v45 ^ v333 ^ v360 ^ v380;
        v157 = BitOperations.RotateLeft(v156, 30);
        v503 = v55;
        v199 = BitOperations.RotateLeft(v55, 5) + v361 + v236 + (v157 & v459 | v280 & (v157 | v459)) - 1894007588;
        v460 = BitOperations.RotateLeft(v459, 30);
        v371 = v47 ^ v342 ^ v370 ^ v390;
        v56 = BitOperations.RotateLeft(v199, 5) + v371 + v280 + (v503 & v460 | v157 & (v503 | v460)) - 1894007588;
        v381 = v309 ^ v351 ^ v380 ^ v423;
        v504 = BitOperations.RotateLeft(v503, 30);
        v237 = v56;
        v57 = BitOperations.RotateLeft(v56, 5) + v381 + v157 + (v199 & v504 | v460 & (v199 | v504)) - 1894007588;
        v391 = v317 ^ v361 ^ v390 ^ v400;
        v200 = BitOperations.RotateLeft(v199, 30);
        v58 = v325 ^ v371 ^ v423 ^ v430;
        v158 = BitOperations.RotateLeft(v57, 5) + v391 + v460 + (v237 & v200 | v504 & (v237 | v200)) - 1894007588;
        v238 = BitOperations.RotateLeft(v237, 30);
        v59 = BitOperations.RotateLeft(v57, 30);
        v461 = v58 + (v57 ^ v238 ^ v200) + v504 + BitOperations.RotateLeft(v158, 5) - 899497514;
        v401 = v333 ^ v381 ^ v400 ^ v415;
        v60 = v401 + (v158 ^ v59 ^ v238) + v200 + BitOperations.RotateLeft(v461, 5) - 899497514;
        v159 = BitOperations.RotateLeft(v158, 30);
        v431 = v45 ^ v342 ^ v391 ^ v430;
        v505 = v60;
        v61 = v431 + (v159 ^ v59 ^ v461) + v238 + BitOperations.RotateLeft(v60, 5) - 899497514;
        v462 = BitOperations.RotateLeft(v461, 30);
        v416 = v47 ^ v351 ^ v58 ^ v415;
        v201 = v61;
        v239 = v59 + BitOperations.RotateLeft(v61, 5) + v416 + (v159 ^ v505 ^ v462) - 899497514;
        v62 = v309 ^ v361 ^ v401 ^ v45;
        v506 = BitOperations.RotateLeft(v505, 30);
        v63 = v159 + BitOperations.RotateLeft(v239, 5) + v62 + (v61 ^ v506 ^ v462) - 899497514;
        v64 = v317 ^ v371 ^ v431 ^ v47;
        v202 = BitOperations.RotateLeft(v201, 30);
        v65 = v239 ^ v202 ^ v506;
        v240 = BitOperations.RotateLeft(v239, 30);
        v310 = v325 ^ v381 ^ v416 ^ v309;
        v160 = v462 + BitOperations.RotateLeft(v63, 5) + v64 + v65 - 899497514;
        v281 = BitOperations.RotateLeft(v63, 30);
        v463 = v310 + (v63 ^ v240 ^ v202) + v506 + BitOperations.RotateLeft(v160, 5) - 899497514;
        v318 = v62 ^ v317 ^ v333 ^ v391;
        v66 = v318 + (v160 ^ v281 ^ v240) + v202 + BitOperations.RotateLeft(v463, 5) - 899497514;
        v161 = BitOperations.RotateLeft(v160, 30);
        v507 = v66;
        v326 = v64 ^ v325 ^ v342 ^ v58;
        v67 = v326 + (v161 ^ v281 ^ v463) + v240 + BitOperations.RotateLeft(v66, 5) - 899497514;
        v464 = BitOperations.RotateLeft(v463, 30);
        v334 = v310 ^ v333 ^ v351 ^ v401;
        v68 = v334 + (v161 ^ v507 ^ v464);
        v508 = BitOperations.RotateLeft(v507, 30);
        v241 = v281 + BitOperations.RotateLeft(v67, 5) + v68 - 899497514;
        v343 = v318 ^ v342 ^ v361 ^ v431;
        v203 = BitOperations.RotateLeft(v67, 30);
        v282 = v343 + (v67 ^ v508 ^ v464) + v161 + BitOperations.RotateLeft(v241, 5) - 899497514;
        v352 = v326 ^ v351 ^ v371 ^ v416;
        v162 = v352 + (v241 ^ v203 ^ v508) + v464 + BitOperations.RotateLeft(v282, 5) - 899497514;
        v242 = BitOperations.RotateLeft(v241, 30);
        v362 = v62 ^ v334 ^ v361 ^ v381;
        v69 = v362 + (v282 ^ v242 ^ v203) + v508 + BitOperations.RotateLeft(v162, 5) - 899497514;
        v283 = BitOperations.RotateLeft(v282, 30);
        v465 = v69;
        v372 = v64 ^ v343 ^ v371 ^ v391;
        v70 = v372 + (v162 ^ v283 ^ v242) + v203 + BitOperations.RotateLeft(v69, 5) - 899497514;
        v163 = BitOperations.RotateLeft(v162, 30);
        v382 = v310 ^ v352 ^ v381 ^ v58;
        v509 = v70;
        v71 = v382 + (v163 ^ v283 ^ v465) + v242 + BitOperations.RotateLeft(v70, 5) - 899497514;
        v466 = BitOperations.RotateLeft(v465, 30);
        v392 = v318 ^ v362 ^ v391 ^ v401;
        v243 = v283 + BitOperations.RotateLeft(v71, 5) + v392 + (v163 ^ v509 ^ v466) - 899497514;
        v510 = BitOperations.RotateLeft(v509, 30);
        v424 = v326 ^ v372 ^ v58 ^ v431;
        v204 = BitOperations.RotateLeft(v71, 30);
        v284 = v424 + (v71 ^ v510 ^ v466) + v163 + BitOperations.RotateLeft(v243, 5) - 899497514;
        v72 = v466 + BitOperations.RotateLeft(v284, 5) + (v243 ^ v204 ^ v510) + (v334 ^ v382 ^ v401 ^ v416) - 899497514;
        v244 = BitOperations.RotateLeft(v243, 30);
        v73 = (v284 ^ v244 ^ v204) + (v62 ^ v343 ^ v392 ^ v431) + v510 + BitOperations.RotateLeft(v72, 5) - 899497514;
        v285 = BitOperations.RotateLeft(v284, 30);
        pCtx->h0 = pCtx->h0 + (v64 ^ v352 ^ v424 ^ v416) + (v72 ^ v285 ^ v244) + v204 + BitOperations.RotateLeft(v73, 5) - 899497514;
        v74 = v73 + pCtx->h1;
        v75 = pCtx->h2;
        pCtx->h1 = v74;
        pCtx->h3 += v285;
        v76 = v244 + pCtx->h4;
        pCtx->h2 = v75 + BitOperations.RotateLeft(v72, 30);
        pCtx->h4 = v76;
        if (a3 - 1 <= 0)
        {
            return;
        }
        v77 = v445;
        v444 = a3 - 1;
        do
        {
            v78 = pCtx->h1;
            v438 = v77[14];
            v77 += 16;
            v79 = v438 + BitOperations.RotateLeft(pCtx->h0, 5) + (pCtx->h3 ^ v78 & (pCtx->h2 ^ pCtx->h3)) + pCtx->h4 + 1518500249;
            v409 = *(v77 - 1);
            v286 = BitOperations.RotateLeft(v78, 30);
            v80 = pCtx->h3 + v409 + BitOperations.RotateLeft(v79, 5) + (pCtx->h2 ^ pCtx->h0 & (v286 ^ pCtx->h2)) + 1518500249;
            v164 = BitOperations.RotateLeft(pCtx->h0, 30);
            v81 = pCtx->h2 + *v77 + BitOperations.RotateLeft(v80, 5) + (v286 ^ v79 & (v164 ^ v286)) + 1518500249;
            v467 = BitOperations.RotateLeft(v79, 30);
            v393 = v77[1];
            v82 = v286 + v393 + BitOperations.RotateLeft(v81, 5) + (v164 ^ v80 & (v164 ^ v467)) + 1518500249;
            v511 = BitOperations.RotateLeft(v80, 30);
            v383 = v77[2];
            v83 = v164 + v383 + BitOperations.RotateLeft(v82, 5) + (v467 ^ v81 & (v511 ^ v467)) + 1518500249;
            v373 = v77[3];
            v205 = BitOperations.RotateLeft(v81, 30);
            v84 = v467 + v373 + BitOperations.RotateLeft(v83, 5) + (v511 ^ v82 & (v205 ^ v511)) + 1518500249;
            v363 = v77[4];
            v245 = BitOperations.RotateLeft(v82, 30);
            v85 = v511 + v363 + BitOperations.RotateLeft(v84, 5) + (v205 ^ v83 & (v245 ^ v205)) + 1518500249;
            v353 = v77[5];
            v287 = BitOperations.RotateLeft(v83, 30);
            v86 = v205 + v353 + BitOperations.RotateLeft(v85, 5) + (v245 ^ v84 & (v287 ^ v245)) + 1518500249;
            v442 = v77[6];
            v165 = BitOperations.RotateLeft(v84, 30);
            v87 = v245 + v442 + BitOperations.RotateLeft(v86, 5) + (v287 ^ v85 & (v165 ^ v287)) + 1518500249;
            v468 = BitOperations.RotateLeft(v85, 30);
            v432 = v77[7];
            v88 = v287 + v432 + BitOperations.RotateLeft(v87, 5) + (v165 ^ v86 & (v165 ^ v468)) + 1518500249;
            v512 = BitOperations.RotateLeft(v86, 30);
            v425 = v77[8];
            v89 = v165 + v425 + BitOperations.RotateLeft(v88, 5) + (v468 ^ v87 & (v512 ^ v468)) + 1518500249;
            v417 = v77[9];
            v206 = BitOperations.RotateLeft(v87, 30);
            v90 = v468 + v417 + BitOperations.RotateLeft(v89, 5) + (v512 ^ v88 & (v206 ^ v512)) + 1518500249;
            v402 = v77[10];
            v246 = BitOperations.RotateLeft(v88, 30);
            v91 = v512 + v402 + BitOperations.RotateLeft(v90, 5) + (v206 ^ v89 & (v246 ^ v206)) + 1518500249;
            v288 = BitOperations.RotateLeft(v89, 30);
            v530 = v77[11];
            v92 = v206 + v530 + BitOperations.RotateLeft(v91, 5) + (v246 ^ v90 & (v288 ^ v246)) + 1518500249;
            v93 = BitOperations.RotateLeft(v90, 30);
            v94 = v77[12];
            v166 = v93;
            v207 = v246 + v94 + BitOperations.RotateLeft(v92, 5) + (v288 ^ v91 & (v93 ^ v288)) + 1518500249;
            v95 = v93;
            v96 = BitOperations.RotateLeft(v91, 30);
            v97 = v77[13];
            v469 = v96;
            v513 = BitOperations.RotateLeft(v92, 30);
            v247 = v97 + BitOperations.RotateLeft(v207, 5) + (v166 ^ v92 & (v95 ^ v96)) + v288 + 1518500249;
            v439 = *v77 ^ v442 ^ v530 ^ v438;
            v98 = v439 + v166 + BitOperations.RotateLeft(v247, 5) + (v469 ^ v207 & (v513 ^ v469)) + 1518500249;
            v208 = BitOperations.RotateLeft(v207, 30);
            v289 = v98;
            v410 = v409 ^ v393 ^ v432 ^ v94;
            v99 = v410 + v469 + BitOperations.RotateLeft(v98, 5) + (v513 ^ v247 & (v208 ^ v513)) + 1518500249;
            v248 = BitOperations.RotateLeft(v247, 30);
            v167 = v99;
            v311 = *v77 ^ v383 ^ v425 ^ v97;
            v100 = v311 + v513 + BitOperations.RotateLeft(v99, 5) + (v208 ^ v289 & (v248 ^ v208)) + 1518500249;
            v290 = BitOperations.RotateLeft(v289, 30);
            v470 = v100;
            v319 = v439 ^ v393 ^ v373 ^ v417;
            v101 = v319 + v208 + BitOperations.RotateLeft(v100, 5) + (v248 ^ v167 & (v290 ^ v248)) + 1518500249;
            v168 = BitOperations.RotateLeft(v167, 30);
            v327 = v410 ^ v383 ^ v363 ^ v402;
            v514 = v101;
            v102 = v327 + v248 + BitOperations.RotateLeft(v101, 5) + (v168 ^ v290 ^ v470) + 1859775393;
            v471 = BitOperations.RotateLeft(v470, 30);
            v335 = v311 ^ v373 ^ v353 ^ v530;
            v249 = v335 + v290 + BitOperations.RotateLeft(v102, 5) + (v168 ^ v514 ^ v471) + 1859775393;
            v515 = BitOperations.RotateLeft(v514, 30);
            v344 = v319 ^ v363 ^ v442 ^ v94;
            v291 = v344 + v168 + BitOperations.RotateLeft(v249, 5) + (v102 ^ v515 ^ v471) + 1859775393;
            v209 = BitOperations.RotateLeft(v102, 30);
            v354 = v327 ^ v353 ^ v432 ^ v97;
            v169 = v354 + v471 + BitOperations.RotateLeft(v291, 5) + (v249 ^ v209 ^ v515) + 1859775393;
            v250 = BitOperations.RotateLeft(v249, 30);
            v364 = v439 ^ v335 ^ v442 ^ v425;
            v103 = v364 + v515 + BitOperations.RotateLeft(v169, 5) + (v291 ^ v250 ^ v209) + 1859775393;
            v292 = BitOperations.RotateLeft(v291, 30);
            v472 = v103;
            v374 = v410 ^ v344 ^ v432 ^ v417;
            v104 = v374 + v209 + BitOperations.RotateLeft(v103, 5) + (v169 ^ v292 ^ v250) + 1859775393;
            v170 = BitOperations.RotateLeft(v169, 30);
            v384 = v311 ^ v354 ^ v425 ^ v402;
            v210 = v384 + v250 + BitOperations.RotateLeft(v104, 5) + (v170 ^ v292 ^ v472) + 1859775393;
            v473 = BitOperations.RotateLeft(v472, 30);
            v394 = v319 ^ v364 ^ v417 ^ v530;
            v251 = v394 + v292 + BitOperations.RotateLeft(v210, 5) + (v170 ^ v104 ^ v473) + 1859775393;
            v516 = BitOperations.RotateLeft(v104, 30);
            v426 = v327 ^ v374 ^ v402 ^ v94;
            v105 = v426 + v170 + BitOperations.RotateLeft(v251, 5) + (v210 ^ v516 ^ v473) + 1859775393;
            v211 = BitOperations.RotateLeft(v210, 30);
            v293 = v105;
            v403 = v335 ^ v384 ^ v530 ^ v97;
            v171 = v403 + v473 + BitOperations.RotateLeft(v105, 5) + (v251 ^ v211 ^ v516) + 1859775393;
            v252 = BitOperations.RotateLeft(v251, 30);
            v106 = v439 ^ v344 ^ v394 ^ v94;
            v107 = v106 + v516 + BitOperations.RotateLeft(v171, 5) + (v105 ^ v252 ^ v211) + 1859775393;
            v418 = v410 ^ v354 ^ v426 ^ v97;
            v294 = BitOperations.RotateLeft(v293, 30);
            v108 = v418 + v211 + BitOperations.RotateLeft(v107, 5) + (v171 ^ v294 ^ v252) + 1859775393;
            v172 = BitOperations.RotateLeft(v171, 30);
            v440 = v439 ^ v311 ^ v364 ^ v403;
            v474 = BitOperations.RotateLeft(v107, 30);
            v212 = v440 + v252 + BitOperations.RotateLeft(v108, 5) + (v172 ^ v294 ^ v107) + 1859775393;
            v411 = v410 ^ v319 ^ v374 ^ v106;
            v517 = BitOperations.RotateLeft(v108, 30);
            v253 = v411 + v294 + BitOperations.RotateLeft(v212, 5) + (v172 ^ v108 ^ v474) + 1859775393;
            v312 = v311 ^ v327 ^ v384 ^ v418;
            v295 = v312 + v172 + BitOperations.RotateLeft(v253, 5) + (v212 ^ v517 ^ v474) + 1859775393;
            v213 = BitOperations.RotateLeft(v212, 30);
            v320 = v440 ^ v319 ^ v335 ^ v394;
            v173 = v320 + v474 + BitOperations.RotateLeft(v295, 5) + (v253 ^ v213 ^ v517) + 1859775393;
            v254 = BitOperations.RotateLeft(v253, 30);
            v328 = v411 ^ v327 ^ v344 ^ v426;
            v475 = v328 + v517 + BitOperations.RotateLeft(v173, 5) + (v295 ^ v254 ^ v213) + 1859775393;
            v296 = BitOperations.RotateLeft(v295, 30);
            v336 = v312 ^ v335 ^ v354 ^ v403;
            v518 = v336 + v213 + BitOperations.RotateLeft(v475, 5) + (v173 ^ v296 ^ v254) + 1859775393;
            v174 = BitOperations.RotateLeft(v173, 30);
            v345 = v320 ^ v344 ^ v364 ^ v106;
            v109 = v345 + v254 + BitOperations.RotateLeft(v518, 5) + (v174 ^ v296 ^ v475) + 1859775393;
            v355 = v328 ^ v354 ^ v374 ^ v418;
            v476 = BitOperations.RotateLeft(v475, 30);
            v110 = BitOperations.RotateLeft(v518, 30);
            v255 = v355 + v296 + BitOperations.RotateLeft(v109, 5) + (v174 ^ v518 ^ v476) + 1859775393;
            v365 = v440 ^ v336 ^ v364 ^ v384;
            v297 = BitOperations.RotateLeft(v255, 5) + v365 + v174 + (v109 & v110 | v476 & (v109 | v110)) - 1894007588;
            v111 = BitOperations.RotateLeft(v109, 30);
            v375 = v411 ^ v345 ^ v374 ^ v394;
            v112 = BitOperations.RotateLeft(v297, 5) + v375 + v476 + (v255 & v111 | v110 & (v255 | v111)) - 1894007588;
            v385 = v312 ^ v355 ^ v384 ^ v426;
            v256 = BitOperations.RotateLeft(v255, 30);
            v175 = v112;
            v113 = BitOperations.RotateLeft(v112, 5) + v385 + v110 + (v297 & v256 | v111 & (v297 | v256)) - 1894007588;
            v395 = v320 ^ v365 ^ v394 ^ v403;
            v298 = BitOperations.RotateLeft(v297, 30);
            v477 = v113;
            v519 = BitOperations.RotateLeft(v113, 5) + v395 + v111 + (v175 & v298 | v256 & (v175 | v298)) - 1894007588;
            v176 = BitOperations.RotateLeft(v175, 30);
            v427 = v328 ^ v375 ^ v426 ^ v106;
            v114 = BitOperations.RotateLeft(v519, 5) + v427 + v256 + (v176 & v477 | v298 & (v176 | v477)) - 1894007588;
            v404 = v336 ^ v385 ^ v403 ^ v418;
            v478 = BitOperations.RotateLeft(v477, 30);
            v214 = v114;
            v115 = BitOperations.RotateLeft(v114, 5) + v404 + v298 + (v519 & v478 | v176 & (v519 | v478)) - 1894007588;
            v116 = BitOperations.RotateLeft(v519, 30);
            v433 = v440 ^ v345 ^ v395 ^ v106;
            v117 = v433 + v176 + (v214 & v116 | v478 & (v214 | v116)) + BitOperations.RotateLeft(v115, 5) - 1894007588;
            v118 = BitOperations.RotateLeft(v214, 30);
            v419 = v411 ^ v355 ^ v427 ^ v418;
            v119 = v440 ^ v312 ^ v365 ^ v404;
            v257 = BitOperations.RotateLeft(v115, 30);
            v177 = BitOperations.RotateLeft(v117, 5) + v419 + v478 + (v115 & v118 | v116 & (v115 | v118)) - 1894007588;
            v120 = v119 + v116 + (v117 & v257 | v118 & (v117 | v257)) + BitOperations.RotateLeft(v177, 5) - 1894007588;
            v121 = BitOperations.RotateLeft(v117, 30);
            v412 = v411 ^ v320 ^ v375 ^ v433;
            v479 = v120;
            v122 = BitOperations.RotateLeft(v120, 5) + v412 + v118 + (v177 & v121 | v257 & (v177 | v121)) - 1894007588;
            v313 = v312 ^ v328 ^ v385 ^ v419;
            v178 = BitOperations.RotateLeft(v177, 30);
            v520 = v122;
            v215 = BitOperations.RotateLeft(v122, 5) + v313 + v257 + (v178 & v479 | v121 & (v178 | v479)) - 1894007588;
            v321 = v119 ^ v320 ^ v336 ^ v395;
            v480 = BitOperations.RotateLeft(v479, 30);
            v123 = BitOperations.RotateLeft(v215, 5) + v321 + v121 + (v520 & v480 | v178 & (v520 | v480)) - 1894007588;
            v329 = v412 ^ v328 ^ v345 ^ v427;
            v521 = BitOperations.RotateLeft(v520, 30);
            v258 = v123;
            v124 = BitOperations.RotateLeft(v123, 5) + v329 + v178 + (v215 & v521 | v480 & (v215 | v521)) - 1894007588;
            v337 = v313 ^ v336 ^ v355 ^ v404;
            v216 = BitOperations.RotateLeft(v215, 30);
            v299 = v124;
            v179 = BitOperations.RotateLeft(v124, 5) + v337 + v480 + (v258 & v216 | v521 & (v258 | v216)) - 1894007588;
            v259 = BitOperations.RotateLeft(v258, 30);
            v346 = v321 ^ v345 ^ v365 ^ v433;
            v125 = BitOperations.RotateLeft(v179, 5) + v346 + v521 + (v299 & v259 | v216 & (v299 | v259)) - 1894007588;
            v356 = v329 ^ v355 ^ v375 ^ v419;
            v300 = BitOperations.RotateLeft(v299, 30);
            v481 = v125;
            v126 = BitOperations.RotateLeft(v125, 5) + v356 + v216 + (v179 & v300 | v259 & (v179 | v300)) - 1894007588;
            v366 = v119 ^ v337 ^ v365 ^ v385;
            v180 = BitOperations.RotateLeft(v179, 30);
            v522 = v126;
            v217 = BitOperations.RotateLeft(v126, 5) + v366 + v259 + (v180 & v481 | v300 & (v180 | v481)) - 1894007588;
            v376 = v412 ^ v346 ^ v375 ^ v395;
            v482 = BitOperations.RotateLeft(v481, 30);
            v127 = BitOperations.RotateLeft(v217, 5) + v376 + v300 + (v522 & v482 | v180 & (v522 | v482)) - 1894007588;
            v386 = v313 ^ v356 ^ v385 ^ v427;
            v523 = BitOperations.RotateLeft(v522, 30);
            v260 = v127;
            v128 = BitOperations.RotateLeft(v127, 5) + v386 + v180 + (v217 & v523 | v482 & (v217 | v523)) - 1894007588;
            v396 = v321 ^ v366 ^ v395 ^ v404;
            v218 = BitOperations.RotateLeft(v217, 30);
            v129 = BitOperations.RotateLeft(v128, 5) + v396 + v482 + (v260 & v218 | v523 & (v260 | v218)) - 1894007588;
            v130 = v329 ^ v376 ^ v427 ^ v433;
            v261 = BitOperations.RotateLeft(v260, 30);
            v301 = BitOperations.RotateLeft(v128, 30);
            v483 = v130 + (v128 ^ v261 ^ v218) + v523 + BitOperations.RotateLeft(v129, 5) - 899497514;
            v405 = v337 ^ v386 ^ v404 ^ v419;
            v181 = BitOperations.RotateLeft(v129, 30);
            v434 = v119 ^ v346 ^ v396 ^ v433;
            v524 = v405 + (v129 ^ v301 ^ v261) + v218 + BitOperations.RotateLeft(v483, 5) - 899497514;
            v131 = v434 + (v181 ^ v301 ^ v483) + v261 + BitOperations.RotateLeft(v524, 5) - 899497514;
            v484 = BitOperations.RotateLeft(v483, 30);
            v219 = v131;
            v420 = v412 ^ v356 ^ v130 ^ v419;
            v132 = v301 + BitOperations.RotateLeft(v131, 5) + v420 + (v181 ^ v524 ^ v484) - 899497514;
            v525 = BitOperations.RotateLeft(v524, 30);
            v133 = v313 ^ v366 ^ v405 ^ v119;
            v302 = v181 + BitOperations.RotateLeft(v132, 5) + v133 + (v219 ^ v525 ^ v484) - 899497514;
            v220 = BitOperations.RotateLeft(v219, 30);
            v413 = v321 ^ v376 ^ v434 ^ v412;
            v262 = BitOperations.RotateLeft(v132, 30);
            v314 = v329 ^ v386 ^ v420 ^ v313;
            v182 = v413 + (v132 ^ v220 ^ v525) + v484 + BitOperations.RotateLeft(v302, 5) - 899497514;
            v134 = v314 + (v302 ^ v262 ^ v220) + v525 + BitOperations.RotateLeft(v182, 5) - 899497514;
            v303 = BitOperations.RotateLeft(v302, 30);
            v485 = v134;
            v322 = v133 ^ v321 ^ v337 ^ v396;
            v135 = v322 + (v182 ^ v303 ^ v262) + v220 + BitOperations.RotateLeft(v134, 5) - 899497514;
            v183 = BitOperations.RotateLeft(v182, 30);
            v330 = v413 ^ v329 ^ v346 ^ v130;
            v526 = v135;
            v136 = v330 + (v183 ^ v303 ^ v485) + v262 + BitOperations.RotateLeft(v135, 5) - 899497514;
            v486 = BitOperations.RotateLeft(v485, 30);
            v338 = v314 ^ v337 ^ v356 ^ v405;
            v137 = v338 + (v183 ^ v526 ^ v486);
            v527 = BitOperations.RotateLeft(v526, 30);
            v263 = v303 + BitOperations.RotateLeft(v136, 5) + v137 - 899497514;
            v347 = v322 ^ v346 ^ v366 ^ v434;
            v221 = BitOperations.RotateLeft(v136, 30);
            v304 = v347 + (v136 ^ v527 ^ v486) + v183 + BitOperations.RotateLeft(v263, 5) - 899497514;
            v357 = v330 ^ v356 ^ v376 ^ v420;
            v138 = v357 + (v263 ^ v221 ^ v527) + v486 + BitOperations.RotateLeft(v304, 5) - 899497514;
            v264 = BitOperations.RotateLeft(v263, 30);
            v184 = v138;
            v367 = v133 ^ v338 ^ v366 ^ v386;
            v139 = v367 + (v304 ^ v264 ^ v221) + v527 + BitOperations.RotateLeft(v138, 5) - 899497514;
            v305 = BitOperations.RotateLeft(v304, 30);
            v487 = v139;
            v377 = v413 ^ v347 ^ v376 ^ v396;
            v140 = v377 + (v184 ^ v305 ^ v264) + v221 + BitOperations.RotateLeft(v139, 5) - 899497514;
            v185 = BitOperations.RotateLeft(v184, 30);
            v387 = v314 ^ v357 ^ v386 ^ v130;
            v528 = v140;
            v141 = v387 + (v185 ^ v305 ^ v487) + v264 + BitOperations.RotateLeft(v140, 5) - 899497514;
            v488 = BitOperations.RotateLeft(v487, 30);
            v397 = v322 ^ v367 ^ v396 ^ v405;
            v265 = v305 + BitOperations.RotateLeft(v141, 5) + v397 + (v185 ^ v528 ^ v488) - 899497514;
            v529 = BitOperations.RotateLeft(v528, 30);
            v428 = v330 ^ v377 ^ v130 ^ v434;
            v222 = BitOperations.RotateLeft(v141, 30);
            v306 = v428 + (v141 ^ v529 ^ v488) + v185 + BitOperations.RotateLeft(v265, 5) - 899497514;
            v142 = v488 + BitOperations.RotateLeft(v306, 5) + (v265 ^ v222 ^ v529) + (v338 ^ v387 ^ v405 ^ v420) - 899497514;
            v266 = BitOperations.RotateLeft(v265, 30);
            v143 = BitOperations.RotateLeft(v306, 30);
            v489 = (v306 ^ v266 ^ v222) + (v133 ^ v347 ^ v397 ^ v434) + v529 + BitOperations.RotateLeft(v142, 5) - 899497514;
            pCtx->h0 += (v142 ^ v143 ^ v266) + (v413 ^ v357 ^ v428 ^ v420) + v222 + BitOperations.RotateLeft(v489, 5) - 899497514;
            pCtx->h1 += v489;
            pCtx->h2 += BitOperations.RotateLeft(v142, 30);
            pCtx->h3 += v143;
            v144 = v444 == 1;
            pCtx->h4 += v266;
            --v444;
        }
        while ( !v144 );
    }
}