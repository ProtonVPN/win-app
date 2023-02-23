/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace ProtonVPN.Settings.Migrations.v1_7_2
{
    internal class MigratedServerId
    {
        private static readonly Dictionary<int, string> Map = new Dictionary<int, string>
        {
            {15, "ziWi-ZOb28XR4sCGFCEpqQbd1FITVWYfTfKYUmV_wKKR3GsveN4HZCh9er5dhelYylEp-fhjBbUPDMHGU699fw=="},
            {16, "ARy95iNxhniEgYJrRrGvagmzRdnmvxCmjArhv3oZhlevziltNm07euTTWeyGQF49RxFpMqWE_ZGDXEvGV2CEkA=="},
            {17, "m-dPNuHcP8N4xfv6iapVg2wHifktAD1A1pFDU95qo5f14Vaw8I9gEHq-3GACk6ef3O12C3piRviy_D43Wh7xxQ=="},
            {18, "Xz2wY0Wq9cg1LKwchjWR05vF62QUPZ3h3Znku2ramprCLWOr_5kB8mcDFxY23lf7QspHOWWflejL6kl04f-a-Q=="},
            {19, "BzHqSTaqcpjIY9SncE5s7FpjBrPjiGOucCyJmwA6x4nTNqlElfKvCQFr9xUa2KgQxAiHv4oQQmAkcA56s3ZiGQ=="},
            {20, "1H8EGg3J1QpSDL6K8hGsTvwmHXdtQvnxplUMePE7Hruen5JsRXvaQ75-sXptu03f0TCO-he3ymk0uhrHx6nnGQ=="},
            {21, "vUZGQHCgdhbDi3qBKxtnuuagOsgaa08Wpu0WLdaqVIKGI5FM7KwIrDB4IprPbhThXJ_5Pb90bkGlHM1ARMYYrQ=="},
            {22, "cjGMPrkCYMsx5VTzPkfOLwbrShoj9NnLt3518AH-DQLYcvsJwwjGOkS8u3AcnX4mVSP6DX2c6Uco99USShaigQ=="},
            {23, "S6oNe_lxq3GNMIMFQdAwOOk5wNYpZwGjBHFr5mTNp9aoMUaCRNsefrQt35mIg55iefE3fTq8BnyM4znqoVrAyA=="},
            {24, "R0wqZrMt5moWXl_KqI7ofCVzgV0cinuz-dHPmlsDJjwoQlu6_HxXmmHx94rNJC1cNeultZoeFr7RLrQQCBaxcA=="},
            {25, "IJWo5UWjbv8T71AAqcBtYuIGf8aKDLXUVLW82IHJDlaLYzrA5lO_rbrQdVueMrT4AIDYgLRtcJykf6PvS8LE2Q=="},
            {26, "IQpNgbDPcAyAH5Y8nlaKYq3L9uMOW929zmZxe3Re1n5L7fdYed9HVErP1AMV8r9f-h9_Ckglrts75_6xnSMhCQ=="},
            {27, "ha0056vPzrt4ErHVbwEGSfMo-e0__HU2kvV-XfkspMOCkVKYsJ5BaD1KUXYSLcR0D7K0q6_J_Z8HgJdGGxrJRA=="},
            {28, "sW6Msiby3tNWhOQycK3dOolAL341K40KHOPNv5wSVVFZwkayc7PIflVDxGU8oMwYoGVuI8RWz5OL3yomRfcO6A=="},
            {29, "7J7smwDoOZD537x3sohypBmu8phtWjoc7NmddefXLbHy76M8iTpcU9Zn0QsZhN9tRpJ8ILZ2GZVhaeCbku4IPQ=="},
            {30, "hkw1pXa83IP_hkXMWCR5LraS6XIxCjCeVfgiuu3Rkge7pdwFJSoGa4H_9_9-qol9f4Cee0KLNXmiNYCcBRl8Aw=="},
            {31, "ihu53A4CrTd3dTadqbbZOhcnoPZpT2fwUVXoO2nai2IIl9urLn9CU04d8tWtRS4mbZEZ261RkagN1J1l42K6dw=="},
            {32, "Bq1saqZsuqU5bf4pfkaQWs6I1pj4-w4XWMaeYMhsF5AiU5KZw_PFUkGi8F3cPi3wcxhbsyyGMWUGkEgY7pqFjg=="},
            {33, "rDox3cZuqa4_sMMlxcVZg8pCaUQsMN3IrOLk9kBtO8tZ6t8hiqFwCRIAM09A8U9a0HNNlrTgr8CzXKce58815A=="},
            {34, "eV6W5eQXiEchPojDM6SPSy7ph6tkHS1U52TBoZpT_EVqKJsO8rLjHaxS2p0MV9TmugYPdato-OX_NGF-yUEa6Q=="},
            {35, "gzKDANARz0i8OHhGuZV-oFfURju0I3XeW_hNn09g13dS_NJ57UbW420UAcWb-0s93xoav22O_jARq61FyL3guw=="},
            {36, "TZ0gXiJpXxhLyU2NB1ClFY1mkNISAk0vQKuLUV7MLAynE99drRWsw-7deVSaX8vhZ_Q6rCe4GHrF-9LX345S_w=="},
            {37, "VlBt5We4s7pwtJPvy63zZcGYmIOop7-3tiviXJp5tNUjXOcnBSqOcHANGdat7cCPgs-vQasYaqPC8cH8Xj2BgA=="},
            {38, "gi_MHe7rStGdIGADZ0zR5fqgqD4FIjq_G53NRs-2uZfiNqYLhA6YSCTX6Ho_OYEwi0v8NLUDoZFPJZouJ_YGzw=="},
            {39, "B78qtYLE6I1BjXKknSHfCGRBlpkWhe-QnR68jPYnO5clBmhF9AGwBlgt_mh5M9Dje4vuMdz9QyMKXVorCx0feg=="},
            {40, "8gFRboIoqm572UxeQrN7UDWW6YyBNJWxP2lL5ma4kh2WM_AiI408cfxe7DJ2ickJXEm3-knxD5cmWSqd-PMNXQ=="},
            {41, "xGNWMmeX_KbO_XdKtrhbXPWVCflY33SmHPO6k5E3DNM3Qc6eB0yd26UDhRjYWOLAZns0TwVy6v-EiOy2QXWAdw=="},
            {42, "fEZ6naOcmw7obzRd1UVIgN3yaXUKH9SgfoC8Jj_4n2q1uTq1rES78h_eaO3RHAHZ4T5vgnpAi24hgWq0QZhk8g=="},
            {43, "r-cumUipwfofNYhXQWTf36Q9FBpFBdd--ZaLoGLeNGzTpKo86_yqCYWNETc4EubgVm-hgHEqbfae-t4Lw6MJSg=="},
            {44, "38pKeB043dpMLfF_hjmZb7Zq3Gzrx6vpgojF5tPHKhJXNGUmwvNMKTSMYHDsp8Y-n8EUqYem3QMvUQh7LZDnaw=="},
            {45, "KLMoowYF45_Q0hRhQ_bFx11rMIBCm3Ljr_d-U_eDQhbHSf5-j6Q2CPZxffw37BOel8uOoM0ouUmiO301xt_q7w=="},
            {46, "N63r9gPcEBu6cenKrOIjIwPLzuT_So458WgbiBvHbDueZ8K_PQboKAAWu5yH95-3SEk7R4nnxqlU-qhRD07r5w=="},
            {47, "Ik65N-aChBuWFdo1JpmHJB4iWetfzjVLNILERQqbYFBZc5crnxOabXKuIMKhiwBNwiuogItetAUvkFTwJFJPQg=="},
            {48, "jctxnoKsvmlISYpOtESCWNC4tcFbddXmcQ6yyM94YP4tBngrw4O9IKf8jxSLThqZyqFlX972kKwQCPriEeh4qg=="},
            {49, "L4vGcbqflhy1ndWmI4_-upBlUpX2g0uNpxouKVYJoRc02Ew_qeBBGBPavPpv9qhSD-Aa6u-r-_GOSTCL00ksHA=="},
            {50, "Z33WkziHqmXCEJ1Udm8f2vC3Jss9EIkFrgk4_rlSDoVHASjAemj5FsCUTYr7_27bgrbE4whe41PY4TiIr9Z-TA=="},
            {51, "j_hMLdlh76xys5eR2S3OM9vlAgYylGQBiEzDeXLw1H-huHy2jwjwVqcKAPcdd6z2cXoklLuQTegkr3gnJXCB9w=="},
            {52, "Zv2tcvM2nlQ8XiYwWvWtfR-wO9BHprBVm-UxtpNUMlex0M-EEQpfQxdx-dEYscubmbHjMo6ItsHNp0QqTM89oA=="},
            {53, "GsNkHTLamaO5MNhtc9N5DP0QwteU09TgyjeT6ll-ZAOtRhxVTymQQ_VeOKbn1oe_cFazQK5iXsp1YFv2sdDxOA=="},
            {54, "vE3gcSd7zlHF7AehDtvsz-TJdCS_8QZfICQKByoBgYtB6qwW74_Sz5ZHkxWD-GNSrDpD3FsuSxDGJNuSL8K-Mw=="},
            {55, "OYB-3pMQQA2Z2Qnp5s5nIvTVO2alU6h82EGLXYHn1mpbsRvE7UfyAHbt0_EilRjxhx9DCAUM9uXfM2ZUFjzPXw=="},
            {56, "zW5JnLHBgqy4BpXLcwD8-z2pt977MQG236xcN8QdQwdgSVox0sTZjKA5nOhA6CL-RF-R_jYRQFL33linRRcPlw=="},
            {59, "xDxlLnGBlCuWddVcl34X6CnjXVpNeM3RQITuUWVXRxGmbtmV975hb3L9VFm5PErObNsnSDKRB7qRT_goIPvQsw=="},
            {60, "C4R9awiCrxSDSZxdhzZO_rv6vworw1Ox4k_9xH41si9qPA7MCJEX1YNjFj3CovDYCCeVqNN-B_clpOFfmlhPww=="},
            {61, "vXOiCO533JxXlqJsrhvREDwaFhGOXXV7NYjsz06VDJGSnes72blaG8hA9547xU1NLrMsDbA3lywhjPZ7oNYNcA=="},
            {62, "MYGJp20LzzFI8UabkCdSxV-InEpmDQTNT8MT_0hqLvCS2f8B48-osHgPOAJGM0bgpJA1hG8neJPue8wBraqBLA=="},
            {63, "Lab5ug5wsnIedm7-pe5hB6nZ-vVDyoznnMHbCSNoyAIiuMiSSATtyyR-BBMaqIdUiNrYiVMfy1Zmh9L-q74Lng=="},
            {64, "tHdKCeJlgD7mv_W13BqEZeelMiptPIK6r8arzZFcQcLvBLNiogdGOGVyYOldyhzcnSzCPkvkWj-VtyDwSjIncg=="},
            {65, "1SDBH_1L_zUuNzXzQa6JKox3qZTINidjF-5fk4SM2uzZ7UkvA2Dz_Cv1-p-X3toVYzHMwcrVKEjRqIdotiMdFw=="},
            {66, "6opBd5UdUtY_RtEzWLQNN128C-nG87siCoSoDAwgyGLXDNe6fKrFE9XUqmz68E-BAf1wzwipuPU4sbt2hvOrYA=="},
            {67, "Vk-7HUMELOxtoLNAoxS1uCv_XT2jAQOAj-t2BCVOIkr_opyC6FmmVHrvoH4N-54gWpEnQVPcpe6o7dLbV0IgBw=="},
            {68, "tmkbY5-BgxYUahTwB0ngjgcWIWQpCw6Q7DVSLN5PCjiss4-kp-xLRoylmJ0mSPzgyEXDHUCa26gmgNesiG4brA=="},
            {69, "q8pWnpe8LXw5BMAN6XDyU23SE9ecXdB2U_As6yCRjB8wYokK6qMdVgCM6-h2bwRfWQ68raeFQoeaX8yD4NCkLA=="},
            {70, "38bNcNTweRDFCZPhO-ppWbdtK0aFg37NZt3zAUwGbIK_Jl2rB7U9YqrXRIz0Gtdh2yq7vZa5dbMkaUOy0kNNyA=="},
            {71, "lVJ7pFH-QRoQAcjkhfPKt9LJnWblD-bXg41QYWUZAUGwGYkUakaAB79u94pvaKb7SqBgqCbyxoaTOP3Smgm0ww=="},
            {73, "AbDzcisHq0YHVi-3B4-H2_tjuY_WJemx3NX3zAgJ9KMbGWx_yWV08Ms8s0dng_Qt_DvMYfxLOWL2Yw1JgFREeg=="},
            {76, "f1MbE4ydZBpVns9Wz8mA5x0nqqadqF0a61PZXR0CkYEnsBQyiGHvw7-Co6bJ_Wl0bKQJFBCh_ppRvup_P8VPiw=="},
            {77, "s4Suo5gVNLE9UPt320KF3QleE1Zj07AYKG1O6SJ7FUIL9_3Kr73-5pDGZLvVBPotI9po-coIP_S-faG1FcOjlA=="},
            {78, "SKhICvkxYLs-TqZBpvHn9feBblFYXw1LpwJ0LXHRm9lFvrYds7OjJostTsR-PpMQEyYz9QAgEcxaOS9HF7Av2g=="},
            {79, "XYbCWMJRuiILpmb8syJD4mtPyGNe2xrVAcRNmzJRx6lCmWJ_T4r6Mlc3xKqBhnoZdvtf1VXQiNQN8idDy5BWZA=="},
            {80, "cX2Ji0VckbvBzZTf87sxq0_-n10qjuIiAy62NJb0Wz8-sdTbwzdRFrFrD17lk93Knk6sCfpI353FpPA7q0GQTQ=="},
            {83, "v8A0xW5bt0IXa_IIqsZxLgZ4HQaIleiiyKhbdu1OBvlBZfLxN3rdegjGH9y65Jg3u88_wpjoj30yiuUo2Ci1Sw=="},
            {84, "I5iXhAkT2kFuWSdowKPodYnpVBeKqZwoZFXTp9VDTknWpxvsqtFcyHsTwFccYnBo3mD-XsQ1KqveTyl6geQzxQ=="},
            {85, "jUjNWfdzq7X-CMrR3LyX2y7o_D6y4FIGmsuTkfdNOI4QtPyaed-kVyFcOw9XLiRpwRDKnyKtHxhrIBW7nPVZdQ=="},
            {86, "WErNQRoJ-NaSvs5N1NnpRHvobabt5btZXBsmPuqLd5MvTJnefHSZgOigRXjdmfklxa5TSA20uJesCu9TetKupw=="},
            {87, "9-EXVm5ajoie4Fl8qMhD2qpdHkgnFppxB0DjvUhhK5fqJz1pW3fiW2ajv9qkeN73GK_QnEptA69w1RRKvkt4Kg=="},
            {88, "yu50U8Rf9dhPHDcG3KTX6Nx3Euupk4uskAj9V9YVCFSB3oQk8_pTGfWwRFkRPYziGL5EsEx42ZyRHdcZxpO8CA=="},
            {89, "Oq5v73PN6q3DjnWTp4UwxDBKAfSt3cDgVVLjzrp9r9yCA8IuQJiIZKX8ZDQctXWOEFMO1ROxffMjoqwYwg_xLg=="},
            {90, "igpLl8Gtj7Ve_fVnsnvFp_gUne1iq2gZVj87kxaEeoW7WqFAy_f8ys1KktlqPpa0fE_yGaWzuqOKQtRqaLMs3Q=="},
            {91, "CZLsVy9MkpYlmTELt96UJFIcgcNQFPxqEFIkjt2sGRWF3lnZNRWuIFM-sUTzdXnx4f6zUBUNQxd6-l3CuEkvMQ=="},
            {92, "bEte7pTqqDbmna--bKxM_rxlaRux8l7-E6tZ2IsYuakn5ZSJ8t-aQapwueG3HAnTRcPfR9bCkczgZGRf4ZsioA=="},
            {93, "KV8mjImpquR4tux9teokTx9bRB3GDgJYgDV2r1PCaCVD9o8InZs99CZr_q2G0qyP8QES4FZyxdO5gU1K-2Jv7Q=="},
            {94, "Uc0Z4tdf4clwVr4eDSPY0bWjH9kIzvLeQ-qudaHD-WJKY21IfFV2t73civ-hnJN7dtbGpV5eUfX5DOKE_nWWcQ=="},
            {95, "CaioC57NM0BfA3WDRAG8LApascUxftg9U2rl4nU8OrqWfQfm_xO1A03EJR7XBYwGlx81IylQrK5DHhXsPt7MtQ=="},
            {96, "qh_qQPwjLZU-Uzx2oXLq5Bl3eS-gjhYm7mxO5rCBzGMa7oLhNMfshPmpP9QWj7d52NOT3X_UVLujhrnHigo3-w=="},
            {97, "ktqlKw3s-JEjT2s6BDWeuHqOtqmrUPsDcztLoG6l5Ik3CPQzFgpLcRAs0u2qmtEFO3tKZMWtoAUbRsWTKNV5vw=="},
            {98, "q_HAaTGgl4Q3uPTsMITYS6qRKOkqnJ5jbOSXGDc5TR-s-CZymLWQcD3j1AbOjTMEYE6dRSjWDEuJdP9N1mYaCQ=="},
            {100, "NEXN7M46GQlROog4NYwPp4QIeGUmDG7qv4rzp2SqlncVEs5cXPWa7qnYMOxEIP19btBTk4deiaOk5yyThZknCg=="},
            {101, "I1vS9wAKew6h5oqAR3Vbo7p6WosmP-AtnHcZcOpDw22pdyqG3efbMJdSRtzOBrSEBgyaoOK9Ogr3wUaB9ccjxQ=="},
            {102, "Qb2ngNPehlUDEcIOH6T82V8CMDaqN9S_n7V6esTo78tNeJTm3cAzEUXTtGazQgsvpC3MR8yuvOZ0eEnkbK5yfQ=="},
            {103, "JgyK70RpusE8ci9eooA4H7pEabexo1mQoYm4TSv7Z643gHTD-taJMSrbC4qIOGAN3VGvuvKAr9Y2cU6A5X789A=="},
            {104, "A67wsII4qAC7ttBVyOh_1yBrNxEKa4KKhzfbOmA9DxJ-EmwzdpCSC68QYF7V1n3jBJnkxDYrffs5_pJHPI9q_w=="},
            {105, "QTWXZRCtm3JEJq3YEbBC4fO4AAuhu-nU3v09RcMq0TP4SADobZBqKUnnEPYVUn9KIxCXbvmZS7yiEyh3RCNFUQ=="},
            {106, "YXhWTAQJs4oqlZqErcsNsKUpfw6LAiG-KYCgPOH8BY1JxM3aCAbqDJhK3x0evmPTI2_2oysemGTz_w-fJsQdYw=="},
            {107, "Yx-DYR0GKN7lKTrGumfuHTDfzUNt9noyh-KMZP0vwmkrzXh2L9F7c9M8xx7-WVPnjTFNu8wF6xV8K83P1rMGEA=="},
            {108, "cLHnXnFVE5pMZw1vECRGpgjkIrEflLx9en1VvXempQUz-Z7I9BKQu5iCt4fmsYkYE9wQkYqdnKk5aTYQ3KKE-g=="},
            {109, "_O4OvZH6gYw77rrYMbHMOiZ6XGR-gZbUb9KkA4C_8Gm01NmV1lNy4Jdt45Ld29Io2Z9oJQnm9WrRHwYHRSw78w=="},
            {110, "E2QUmfoZFQ6UYGPheMhcKsrQy8z3gKNN2-Kktd6AyFHoryoJPEfxZJRQObsvpyYRI5FtJvd-eF-DR2dZ6Uq-Tg=="},
            {111, "T0cX6TLun8zhYK5a6Qkr-Kg3cEuYc8aLTpNVgPEslZYOLxGFCsxoYoDk3TQTt57c_40KR5Wn1D4pQwNeoWfWwg=="},
            {112, "_q7mxlJlQCjJlx6vG5f-9eV3wdQAz3qICTChemyPNKkCMtMKl3fSvv81QuylU7s8VZIWDoWxTTYg6eSoZmgImA=="},
            {142, "wS6EviM2F2e2JejePNe1kWY64ssCotDYZynKmeuTIA4o8tkOdI7hrUSBO9k2OVJrzTjpQfY07WS8eqD0RhZUCw=="},
            {143, "EpbXrL6uYaa-VZyZ8pWRjrqvGYzUDTvnQDYirhAzbke_l13zP4isQlr97QTeIEvzLSImhha808T8h13LRwHBVA=="},
            {144, "C7yR_PpifbYWhO8J5Mb2F_Mjnc47SvgtP5VgR_hXqdQEV0vxIj_RBKs3VxFt0oFb7EtKnzqIMDJ1kSRFaQ6Fog=="},
            {145, "zGLpXK6l-zrqRAFnuDhQAPkxpCpZp-NleHU-mWLBktuiEBvKpqeNY0HfsSACaOV9IWq6PJpOplT6EmheafuvuA=="},
            {146, "Jl5b_2qQOxxlZQkR3guRG5toOlS_shg2O6fgSxiKKC6GHHofZKhaK4rxB4T1wGbAOh137LYMMW4zlaJBV0SmcA=="},
            {147, "pcCWuJFG38pl2T1bOFV1sZEwvFVZsbsDgoWx1SrDIWmxG_PtJyv-XvtTYK2vgcXRhOMNo8_D5k30Sh1KkWkjrg=="},
            {148, "tONw2MFPTA1N45Nez778hiQlcbmpvjECBh_TB3Vovkacih2RTfxXoQ1_x79XTslhl-PyEvGq7vmj9xPvGggsQQ=="},
            {149, "XxDjOt8cH8gFyTHvdhRinwaNF7JjQw_7WRv7vDoauVtIBV3qN7scjALTlGC-PCZQdnieCb6IkZOOF4XBUrlq_Q=="},
            {150, "YFPgQJCQGwjQAbyCHtB4g9f1AMJp-xoGlsWV4PLwJtHw-Dcspcf5YS1cKlCtvSu-aZW47ryBz5nKPJss1EVurQ=="},
            {151, "SlU3wQzhOieDL18yX39n6IlsZTRHP66ealKLpP3wRiDItQuTfyGoC0_HVDqhuK_Khu9UdIKBHjyRMbWdbcwkjQ=="},
            {152, "8XiPu2ESH0R75Uy1GwwsyCozPE-s6Kj2bzI6Y6y6cvMvGNS_mg4jyn1ISvQCXL2meKCke1Uhek10wthQofxI1g=="},
            {153, "2fzIRNasmFQoJWJlGfdZsWn2Ml_5m5L6fNT6nSiOvKkPs12exOlDe04jbp2GSZRXFnj_iva7h8jEKe3d1MVAvQ=="},
            {154, "8lGwDMA9JOb3GYAZHGb9uNMqdUwA8RzNebI8_FyWIChPP1l1Tg4eT0Pc1WSWMgdenkhg5KSJASqua5jYF4s_vA=="},
            {155, "WGpE3ZSy9d-zA8Ciw7vwz5mGHMbUIbAMagXaEmBQWyjQkcPTE55Hvn-OLNVplzYHa_gKqCmgfITdnNHFrcrDSg=="},
            {156, "n8rT_cAdamsvX-hRp0NJC1QQAJ0guJWlQ3CIYw_MWCOmfewO_8cjyFG6YTTL-_Hl8nJjhdf39cSDXfXn9XcurA=="},
            {157, "lRE3UwLV1Do-w3DiOBbC5SW4vWOAli2YGqnraOLYbylmkM8RBpJVijqKjz5CWroZ2VozjSboH7uGXlgEymGEOg=="},
            {158, "B1rzrN2A-VHnlr1Np7MvnGQCJaTFXLdTUJQtwsnM9D0k9MbHy5ICiWlnXvMULukVnb3MsmTeiloOi8M0W2KJdg=="},
            {159, "wAa_BjR7CpGe7fUwb4XF4QAmmDdk5TAFnDDJEoIU-gBDZ-wZMc_MEM4K1mQKZ6fEZZshH_Lkzn9El0BlzaeoDA=="},
            {160, "gze3wfvKprRzrcioqFxUqRsb-_LUQuigw0oMLznyk-Rz9SpNarGuYvC34Uuw9jwbqt_p4LfOKwhqnB2DsX3xOA=="},
            {161, "BcMR48g9A40xFDikwlypz_0HUcAuDg4JLXaIXnU_3GTaHhMMpDAYmwLPSpslKww7jM980F3yTbkdDle-PQ1ZmQ=="},
            {165, "yvDf-6ODMJ05KiDy5ForI8AS77lznK4yKvaQ2t2sKTsWlfd-UDfMnZpllpZcvKsuuNXnr4No2NArIu4vN37_zA=="},
            {166, "Rc1jL2pgiCS80W-hQ9FXPDU24H3w17cSKsoMPzjzBDYeE_YeSbKUcDV7BzfUpUQibfRP0flcI8zcIkhltwDaYQ=="},
            {167, "M0prDxG_M62NNnt_YYUmDifQTdLIsZED9sxRIXC4NscVDojDBDo1P04w-cRFYjkNFv35hV07jP9RzcOUTxE6Cw=="},
            {168, "T3bFSHk4Sz7kkijmUnleDQpzWoZAPOaJY-ezFjkxdVB59YJdlUPgqeywUDpkzh5Cjaih3jYnSCLMmZ735mGTcA=="},
            {169, "ZZeyltOKKIjBtob_fhfyZkXU8sTdAnQv6XDwRsDQxfwXrpY156vujloS2VhvdCwyOCP21uO1EGXQR6JyrlCn3Q=="},
            {170, "o5MB0znIKWC8H7dQ5UEqq8eIny-YTUiN7IF0nTdr1yv8j63rBtwtxv6UeY7kRRjA3i2JTg32V7rhvumdrOn7oQ=="},
            {171, "hlL69Ng2E4bYIp_tx9Edt4AIYqc5nzk89dSZUN8d8Jvrs3mLtFXlgtXtraV2IoYN4ame3z8Dzvx3EXMM2w2hYg=="},
            {172, "9Dbrv9mvQu4tGTrtjnMb-W2Vd5yH5Vg8gnEs_iREWQa8a7HpL-y4ZJTPjPBCxWXxW2egy9sSmi46hvlRHuFxgA=="},
            {173, "uwF9PMX5a9hSzVxE2IsbFLu6NPYryEjc_lM6Psapup8RcuRcY7XVhDKPZBphM8GnhcRaX5rxi24jaNZc9eukKg=="},
            {174, "ocxvqJ98DYNwBWIQBS-Hz4PNaC-D15Qm3rguf0Q1pih6taMWasi41mOUSGGwtm-GWvFegocnwXf8cDd6YcjiuQ=="},
            {175, "GJDFq6OHCMgyZQhMg8GZjsXlF_PRZ0tmkGiCYvlHTTHDey8eVOmB-tELi2awY0unuzgi-jUjhg4WA4XXXiWv7A=="},
            {176, "4Q2Qxw3nW2DG1l1fk8SLIsDvYitpg4CcZI9WrF1pBAnNfYMyR7mQdJGGBkuig0vyZJsmJ-CIe8Z-7uK4e-g4XQ=="},
            {177, "LV97svEYE6PZ4rjeT0FwZ5Iqni8UipJ38_mvPXs9Q3s9w4ItLaXh0L97jI3NBUe9VG5FxVjdG3atJ0pKTrftqw=="},
            {178, "1KvjNslp9smK3cA0JXhCRAug-S7Dje_rg1lnRe2pCWI8HxwZ8tfp8Ii5qoqeDcBtpYltIsYYMsIvlvCk32Lytg=="},
            {179, "2qf5zHWX_gSnFb8SHvLDBPIDhynjf3tvlv4-CBhXF6K-ZpROISfDmU8yDFdJ1gpKDMQgOpO0YneL04aOXEfLrg=="},
            {180, "kJ1HMEZ6D6mf08-mWOIHwqC6VtPySj4EMJ3X9-sV2DiX25JvTAX9PO0lwP2mMmiLVz5uVXACnmueOK8zrpD8Mw=="},
            {181, "jhRK9RMDowi2TaAIeOfK3xlOizL7ox_JrO-euMCD1h4HiNo87_wRsbPjvmUACGttSPfpnySVExjxIgKR7YMUjA=="},
            {184, "n6_cQciFQkEoTpy57hedhxZic_6GXA3MZgMRZAa8TMCW8m78md5y43YNhbCepjjyCibOxbXRwpW0tPaVgN4e1w=="},
            {185, "Pi2oh46FYf7bW-UvNWGdjyDWJHKvT9JAAa2w2eQKG6qRRgJlGDbCMCPW97luYzrfdO-AS9XhG0v5mqiqRaMDWQ=="},
            {186, "VarpLg_dy2R1z70xg91PGThGlS1vPLy7nfB_0FnqM8QU6aqgVQ-JGee8oLNa8VLaIn2PMOvrX_m19cqv6i9EtA=="},
            {187, "fGOntxmxwfBlxhQcaV07_pQLAglcXN0rSPOr4j6jtMaFdVzJDjldRylA1SRxXZdsvmBr7d34lg_q-VPrgctZXg=="},
            {188, "KG9UBxKXBwxCG6otDkiBFKgaOHzWxJtqi3AYBzvinGPG7YzJmP9CXMurO_Z4XKzQCMFs_RkLyXlkQKHqaAgXsQ=="},
            {189, "ZnuO10qYo5SKel88eN79r4tFAr4yBJhENKgxhG-IyRWdoYRGRjhjVwTh0g_gwBWlvWwkRDez6hTGUVNs7bEzJg=="},
            {190, "Y18DHsGY0yYsV7Xcg_rQIKvqHL42jQNc_BNZULTSAAINUC8R8yVIiFkDSUz_kKwE6NAbRl0UKmXnI_NMLl6jTg=="},
            {192, "GHb-EMNhUaRDmmRXI5yFVrF-1wsED2GtmMmn3zaXldP5H5SxiDcl1I2hGTm_lNcE4L6HcT2uUwDh8Vm1U3IZbw=="},
            {193, "l9MvEUIomf-av-zYM_uSqA01nCtWXAMOrx9OzLRSInY-NA7awCRmdDPC8jN4WHk1DVh6IR4weak57mk3RtwDzQ=="},
            {194, "8KjC3mH70Qk8OKXmHVcJEeZieSonJo_rlem9H-ED5xk_4xaG6VIpAXPGDUvKpdQAg49TluACcccKV_kb3QykoQ=="},
            {195, "arLNzTbxhW6aDzNZhMWFRcJsHU9kZVr2m-OrdYmYYl0tOsXf-l9NSd6vPap62HCDhu0QZF0Bql51eJXAnCH5gw=="},
            {196, "icz-n4fK895K3GucbJEwP7xkJIRf6UgvflNk2qL515roPAiz9ejQmzdBDQN67f2QpUn7Z3e2BiP5l3UfMuL6Jw=="},
            {197, "q9e5XkG6c8OsWuNXbiP4JZDAtEOC9hJ6XmPE6UuHRf6iVjYb6BMzg1umYMmrJrkZyW7QiwF_8Jmiyctc1nQiiA=="},
            {198, "AnlxExXgBPAqtQb0nO-Q5hgN7WW30nGSb57Q88tXyy5B50-j59VASk8hrjBya47wLa4tltW8TZ3JVaLxgd86IQ=="},
            {199, "gGxo9cLBMup2K2L6kR_9Fgsjo-bRhhJFbDiLZY9vcFWHKTqmal1i6KjLp5jb2w128-ZEeqQwm1jvkFKKpMzPMg=="},
            {200, "38g3A5xjaoqFPg_vup9c1gHiJonqDNSOwOdZPd-Wjp-nzHQV0lXoVdq6GS11-g28B90viPUxMW3nQ9zgk0TzmA=="},
            {201, "JsvGDqvIFkSMqxWRxG9Aq76OlR8t9menVbq5BVy1J83vz8wdZs2VOb7xyAgY4QaRWObFvpHj6Q0MZ3VcWioe4g=="},
            {202, "NRbH7Af-QxnFt1kcoT-nAygFy_KAd7mqUiGt3LJEJ85E2ogtunGUYM8oIrEVt5ObwT7fp_khOgbE9aWBXb3t7A=="},
            {203, "-o5Oz-YBR2kyJVKFXvpQIoBRIf4YpN3j8B940Vp7u29MRNfKPJMVnKN6YnLRXJ0gYUIZWkXOtIc8m2cYKnH2yQ=="},
            {204, "AZsrdLEdE3a6oDrQ_YbYWXoQ78kNWkiR0t5idbATY3ZRXq922VUZvQ-7uZ44OGasJ1lT6slNB_6yrzEhNC--iA=="},
            {205, "N_RkrPd8rv_ROy0c97jLs0qLUPy78THKRBMTk7w2JViaz5ltqMKR8TU-77Ow6tkcF71FNdmdy2mjS-uPQzf9Fg=="},
            {206, "r5oCGP8TGkQoSMsAemgVclRjSZNoWZotDC60u2zwhK5b5z7YXVVvTDUwp-BUaxkTdI97Ji7sveFKr8Bq8M0CTA=="},
            {207, "dvLSFfeB5eHp1-3UwyDNSCwVW7NZBYcAaG0UVwVqspkM5YiuOcyT_eD8teH-czJdlGY4ctPknEQKCs6IMR2F1g=="},
            {208, "ZrKSJOOOaz2mhYnzjxa8CDjbGIG0ijTbxuhpJ7_-E3EGE7weR9vePnc5_c0bPgKHNct0meIUTVPSQQ_OhyffCQ=="},
            {209, "TLFHEdcwZDgQmtnVUP1K1AryVyq_dfMWBP9KCf6pSeo5jKS5o9ocD0-zPHE9IYTluPWfzSqmfpPBijcVrEAsLw=="},
            {210, "s59Vf7VLZEZ8k1wiYHnbYcVEKaKdd9RoUz89XoeKkKDOE4IDP1I_PhY16K3GrOCNgEsBUFdlzfW5wd1oqoKF4w=="},
            {211, "Lz10W2thf1o8olqvfvKl0mMQAZ0GZpDvkKcroDXeL90j9W2g_uelUOPgl2tTTjMBXkDdT2qS4WcxeQnQSOVGGg=="},
            {212, "3KczVDMHeUOC0l1SijmehL5Z1svVieFur_KcHbLqRIu5QwnoJV56FUcbrP0GjvLN5lGV3oDXP3zjyhax23tnmg=="},
            {213, "OIteXLpQ7eBXZmuvxMkZTUC7haGRVBveF-2lGDU9yV7zIN4qk5IFR_rJzSpnibtJtxnSlM6pel0PjfRNkKLBwQ=="},
            {214, "t3GIxT66VHXXs3QtvnGL8FLhF1_YQqdRBSMLJYIeDjvxEJb_6Nwt1CCK4Z2CZ-gGLKYPGTkKYZNiLPsdv2U6SA=="},
            {215, "Pvdi9NYx_j3ItRuBCXYWXfLdyOkR-qx6VvgJqd1qsReU7qANuoZHj1lWOZhpnlvCIhd2RXLgtbBl2CVa5JXOCw=="},
            {216, "_UWScuhnaUSEugGwH1cSsLDeNkH3qI4uuiUh1Cty2svuXmVFWSzaU1ntJBS4KaCXGf_jRs_cF4I5EaIX4CVPUw=="},
            {217, "UzZw9WZrlWIuw4ubCYPIPrUmvZZ9o7av3kwgC3P4i7mLoIcpYTkyIViczQfQybuBy05fTeNbNDVgR0-9cJk0Bw=="},
            {218, "HtjRKy0FNxBVw6z4hNifZKgYTqlFz8Njlva37OKPp1DIh3DWzuKuKtuBjsP1Byh573sxA2wAGHkssVAA0n1SIA=="},
            {219, "fHR97Meg0sNme5k8IFa2umNtk5FjTUA7FbImbZj7RIO3U5hMmGk8_NF6a7qgSZ2QviSQmEg7Qib9xfLEdjCdXA=="},
            {220, "WGF-SMjNfGbDox4UxGD5GwPUSdi7VIDAeZNl4zDxqhqz0ExoHgMh7k_tDyc3U1MzC__Dpo45t8z41kPtcQv_iw=="},
            {221, "Oa5QYAQwqJaJUXWudgK0jWepj2VBWDQ6aA0bhwQO9SFo0TzJqG9aTc5eQNttv1SHOZtMmh9pBmTMClp1BTOf5w=="},
            {223, "wvtUfYFHDZOHFblSfKvB2C8ULjc9p1R8VBhupZWnwbB8H7FQAUqBlFlQOm5gKvr6QLttx0rKa0MCn4tf3QI48w=="},
            {224, "tBG9buCiSORChGIlZ2XDqx1EgD90qb27q_hGZ028uiT2kinNdE8eEFQZz2Ytglbxdh_wSs-6ZdAbbyJXAZeS-g=="},
            {225, "w0F4zk04gjMl8TcIfzMnHTKfG6meA8UK819CbagzD8xxbL53yR3pCcIUA2JZDAI6wnLRtzMrClbQJOdu8fUTSA=="},
            {226, "kQ_uvjR1ESHZNmeujvqe0osDWhfXo7hCr2bYrBfb7b6Qi2e7Bx6Ufjy-zD-IxzGTkQ5V_5VVC7bjqqUS2XGNHw=="},
            {227, "fpvB5asPfAqXNZXCbSV4r-ZUxbHQsllSqheszXi6W-8pu3jXTjIZEYQglLkcRME7TijvakZSYGoOSdIW9CTw-Q=="},
            {228, "-IjWN9MBEdKAc048v0ebrhBfVhU3kjhdMoexQiTMXtQSkbW-9N59HuiVN6DFMqylIMkLTZoj34bMKxaxPnwfwQ=="},
            {229, "wUtKGRn-o2f6xEi9QY41pWkSe083gqB0S2M3oluhrYJniFunMulS6ohZj6BDHnt2motgpcS2AlROnSarrjXCVQ=="},
            {230, "FQWto4ErygcBaAGx2B9_q5lDm89zizofZNoSaSyN3nBFG_yOnOFetsFszTiLoYweugWo_FUNj8iznDSfQVVZIw=="},
            {232, "jy8HBEWo3LuvwB_kTzk0MWZ3QkHZYKUSWA01MqMt6a8V3lSHjDZ-5EjRLf1EJ22qE4HmY5B5f3DylQGS9dn6FQ=="},
            {233, "tXDvpcaG8ntCuTK6pjl5rMSaQgdsdYe1qlgOduoO1yOWJwOTAnd064Sh1GJila__N2znK_sp43tnNGO_YngAew=="},
            {234, "7GerCBVsrXBIL8f_43e9QNsUpexhLAW3tIdHMxpu-hrba5tCsEnbC0TaLmvAcCXcEZbdwfezyvx3WM3iysICMw=="},
            {235, "HROlHd3-v7QF427YBr4XLFOsEMCQNxViJob2GDheYezlARtFo2JKGYjerKT0GosluAWg2WAob_opABH7IrNMZw=="},
            {236, "1x63GL5P8rSb3gmdvzmtzFCxEcKw-JTdfJTeQR--a2m_P3yJ7cULd7lxRq3oIQraUJaeAfI0ZflPtDxO9jZoWg=="},
            {237, "GsNU4Fo0OcOwGTo54vgbXxzJWjgcqnVcONex1MwGAOA_uQN5gJOrYIIghHF16-lKxyiU4HJwW5nyOQ7MNUIIyQ=="},
            {238, "KEqFFCXoMA3aA8KpzMxM6jzP7iZocmg88Loof5tZKAAV9TWboBgn-0Iu9hPC4s2DTRtX9EOq4Et3QYTsHKvvTA=="},
            {239, "Qp0OXZ-UBTm6FLtizAI254a9Iy2B60F4QhMl7ukGSHKU-bzFC-amlQPqk5GGK2hqNitBw2bvaaHyRcEwjSQj2Q=="},
            {240, "uMWqCWKSkZIbU_qfzOlNFasKE_Dntzjy8goZrNp_ptuDS9gd9do9vpnwTE_TgXih1PZzSWTX1XZUwE0SOv_GNA=="},
            {241, "sQmnUmLlVni9TXM8ygBJjV-7oVAQ7YZZ8Ram9vJ886rB0iCwJ6d56UfIc39ZVguWWuQq_tE1-dHFc08aQ6Fkvw=="},
            {242, "a0kRBJzVB3r_dF263Ye0T831R1LzpvHZ7V_1PRux8NgIbZxzIDOTThP5wFPvRI-hqh3r7DXahjKs2R8W2EoESQ=="},
            {243, "_MS5ufP-az4Cvb9CWLL-b91xNhtxpl0yarmpPoLt2NolajSgIlxqvSaWUFNHqQz8Q38F-2fjOdMj-e5vGOa05w=="},
            {244, "mX7EW68YKkae07bYpODsGJAjjcSIKqSpmAScQZu03ZM2_fhxoLKmUl6SOpc9dp9osUizKS-ArxiTpe7oasqlSg=="},
            {245, "llifYLszIFfeyrnmxjyelNjq0GSMkgSgneSj6wIKQn6GE3qCfYAQvg9mX-_uRb6ra3wby0v3RyfhwTzMVTvRsA=="},
            {246, "bmhdxUo8BZGXUQ3XrXrvkIUCOZ9OFWgnXIlVkXAMrL8jd0W0Q7cvfzvljfWYvGla9tUVeVk0emJ5X6-Jmn_Smg=="},
            {247, "4tXHlUmDxYGFHK4pTuwnuF1Q-Dva7ICJ4DUf-BvetJ5pKCZat8ZD-aXXCxluGsXLIS_TL0L1y00qJDTZOIjH4w=="},
            {248, "COU0ZuFlTagVU1gHFpAHnLdZhJUGHgfBl7wNnIms6pQPr_mKijkRsm2W3Xz88tiTie6CKfnqzWxZoe5IQ9_sew=="},
            {249, "Md7jGBxxD7yxoGYo_3DCatHNWG8dnT5qWEJa-d8BChndFPBrHZwh9LeREbAdpk6-R6jreeg7mwqYMl1zaKpK3g=="},
            {250, "9nnrfzda9i7e2xORiVPyg-QGkcqAEXym8JI_WPVDnBnphtHoc_HbWI8SCfQXJ_lDfHFICIWCURwHQQPX0HOAIg=="},
            {251, "eue2ktftQe8i9KpCCnklR9T1IVf4wB1UIPFoh_daDivm-UkIFMhE7IrOh2-7iSQ9VnXCGlVwC798YEB4KyXs_w=="}
        };

        private readonly string _serverId;

        public MigratedServerId(string serverId)
        {
            _serverId = serverId;
        }

        public static implicit operator string(MigratedServerId item) => item.Value();

        public string Value()
        {
            if (string.IsNullOrEmpty(_serverId))
                return string.Empty;

            if (int.TryParse(_serverId, out var serverId))
            {
                if (serverId == 0)
                    return string.Empty;

                if (Map.ContainsKey(serverId))
                {
                    return Map[serverId];
                }
            }

            return _serverId;
        }
    }
}
