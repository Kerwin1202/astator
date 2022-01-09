﻿using Java.Security;
using Java.Security.Spec;

namespace astator.ApkBuilder.Signer;

internal class Util
{
    internal static readonly string PrivateKey =
       @"MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCf8G39PQ1lMQvu
2u63AuNQU104alBhEs71X4/G65XzjSdSe0YQ0XZAuOR5hHaMYHayWNMx+1HjKLOO
2jHPiRGU8GVyd/QMUw/EJ5IJMbadH6tFS6mxa3K8AEanUMlAGyfOjA0fHdg//ghQ
wifUKfcLYSvbj9Y8MTbe7plFghrCrY9whWdmR19rqaKNVYtBWMNrWg12NDb0+f3M
BL1uKNUD10FJ5dcD/+Zl8N6iOWtHUlofsEiaHa50IiQZN4+eyc1fIRwDAonnTXWE
Ki1ngRISsueY8MNA6FUvOEPQsFfQEm2n5IMBNisQN0YC1VCcLl7L1w7VWTTdJjpx
8dPdn7vJAgMBAAECggEAXGwEMxVHm1UHT0RmM41QrAcwVDxjLGVBANvy6oESisXj
li80It3XlrkBFhNsqdvIW3Emwbg37quDbyY5KHNNnKJ6DPayKTkKYFB5fCSlIRC+
2PFLIwIzL9589YyibxACJcIahwOpBfmW9ovrlheV8ZZ6UItCwk+yJIr+OQNg0WOp
5AONI5c6Fqg6TlxlXXk5MuTf9zIGQtRYtwZH392HlpZJBQEiFFlBIB4s0W4YrCvA
+8zTQHytpaHHWfPvgER/x2ibYtSJxufhzNFxVM3/IYyRfmTxWxEAZ0Xa5hTF5pFD
HOzt0SF9r9GifR3DuD5HEyPRpmrsEJ9NcdVVhYlj/QKBgQD01sY03UOONS7QaSva
fVr2ClQF2ru89I2uQlXmk+1tsiwLpt1F+FEsy135udJTYTYy6L1JradGdFpnplq/
9dQZs9RAP0ceEsnRJU8De7fu3eVWvhap+LOq95q3dvtSL4w89EstdMM2nnSAdB4+
tIDpbmeww6Yb99WtJIDC+JHPewKBgQCnOuP4OWw0Ppici7DojhW6NRrNsho6fpH3
PuZBt401HrpVdcFfNY6iEgmcKxvgYQAElJt9blh5FfaFT5aIiDjcKuwlxI6sjxK9
9xJINcF/2HIuEUySG0HpMkRFQhV4hVlGCnt1KDFI5HBAvG4Lh8v2lsygfvl4XE+/
QaW3MXX8iwKBgBX2mkstuhysqqlpddSgwCMoJAduar71lSwXqUsI5BieDhNxgZIA
R4/kImb/g12Vb947QJe2azYHxABeTO/BZqhzmEu3IXMSLmaDmUXvuD1GTIduf+v5
cmyYz5k7pCnoOAVTyNaoDcb9iefoArqPSK0oCUczFdiWb+WbS62xzKq3AoGBAJBf
PB65XFwCSnij9LfgqotWe2xnvVMQoG2GxPypPWRFwfIzthy/PQNYdSl44hklRQGv
VknEcOcgefJ/UmNOz1/sQnEcr1v0LOcJnaPvnL0FnRV+Y8+jbpSni98K/UROQ1M/
i3QKnfnjFbIduxeDRif96m0JEdDCSSrBvxJXxhefAoGBALgLekh2cWYaBsH1yWxq
WkwxBuxhbtXl0kR8C0HtQsndj9zKvuTytgKmTZBwholahgWuRMp4f1rKtb9/M4RG
uDUfcUmsxt8GXnz87wDPmsirEQpBbFxaeab2dOEQPOdv8T53FtlyJFBiT6TicURw
CO+Ift7Aw4JnJJos32FsOfXm";

    internal static readonly string SignPrefix = @"MIIEQAYJKoZIhvcNAQcCoIIEMTCCBC0CAQExCzAJBgUrDgMCGgUAMAsGCSqGSIb3DQEHAaCCAscwggLDMIIBq6ADAgECAgRYI0cmMA0GCSqGSIb3DQEBCwUAMBIxEDAOBgNVBAoTB2FzdGF0b3IwHhcNMjExMjI3MDgxOTUyWhcNNDYxMjIxMDgxOTUyWjASMRAwDgYDVQQKEwdhc3RhdG9yMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAn/Bt/T0NZTEL7trutwLjUFNdOGpQYRLO9V+PxuuV840nUntGENF2QLjkeYR2jGB2sljTMftR4yizjtoxz4kRlPBlcnf0DFMPxCeSCTG2nR+rRUupsWtyvABGp1DJQBsnzowNHx3YP/4IUMIn1Cn3C2Er24/WPDE23u6ZRYIawq2PcIVnZkdfa6mijVWLQVjDa1oNdjQ29Pn9zAS9bijVA9dBSeXXA//mZfDeojlrR1JaH7BImh2udCIkGTePnsnNXyEcAwKJ5011hCotZ4ESErLnmPDDQOhVLzhD0LBX0BJtp+SDATYrEDdGAtVQnC5ey9cO1Vk03SY6cfHT3Z+7yQIDAQABoyEwHzAdBgNVHQ4EFgQUPYM/w+j1HH67XbKmMmKPOeicbfMwDQYJKoZIhvcNAQELBQADggEBAHlYsIs4Q1ptTv1LUkf5G6KzcDye0/qH0wHkegIEMysd2ZkVZBEFZhbIXgc5DN44fWILI3uu8YTdfpxvRt5WKYzZLKghTHBdym481JMnlM3nkR10hlQFIqDdH7h2BF1lt/D+hWtMkVfaXNfRA54ii39ftRBUCV8TZMz4Ye1xUBsEU7DPD7PKF25eUGGG6kyY8PMVfyv2KKYUWtl5Eumwvs8m/PVKeUhRYJUcap9F0DJ5kidXKkiDAhFeoj7PdOdV15/arbpGqE8/KQWSno3H6x9eO9BSsmntmz0qzds9gf/dUs4LqHJbTQhV+BWgzuov5VsgH1B0czK0kIopk0CfeZwxggFBMIIBPQIBATAaMBIxEDAOBgNVBAoTB2FzdGF0b3ICBFgjRyYwCQYFKw4DAhoFADANBgkqhkiG9w0BAQEFAASCAQA=";

    internal static readonly string Certificate =
        @"-----BEGIN CERTIFICATE-----
MIICwzCCAaugAwIBAgIEWCNHJjANBgkqhkiG9w0BAQsFADASMRAwDgYDVQQKEwdh
c3RhdG9yMB4XDTIxMTIyNzA4MTk1MloXDTQ2MTIyMTA4MTk1MlowEjEQMA4GA1UE
ChMHYXN0YXRvcjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJ/wbf09
DWUxC+7a7rcC41BTXThqUGESzvVfj8brlfONJ1J7RhDRdkC45HmEdoxgdrJY0zH7
UeMos47aMc+JEZTwZXJ39AxTD8Qnkgkxtp0fq0VLqbFrcrwARqdQyUAbJ86MDR8d
2D/+CFDCJ9Qp9wthK9uP1jwxNt7umUWCGsKtj3CFZ2ZHX2upoo1Vi0FYw2taDXY0
NvT5/cwEvW4o1QPXQUnl1wP/5mXw3qI5a0dSWh+wSJodrnQiJBk3j57JzV8hHAMC
iedNdYQqLWeBEhKy55jww0DoVS84Q9CwV9ASbafkgwE2KxA3RgLVUJwuXsvXDtVZ
NN0mOnHx092fu8kCAwEAAaMhMB8wHQYDVR0OBBYEFD2DP8Po9Rx+u12ypjJijzno
nG3zMA0GCSqGSIb3DQEBCwUAA4IBAQB5WLCLOENabU79S1JH+Ruis3A8ntP6h9MB
5HoCBDMrHdmZFWQRBWYWyF4HOQzeOH1iCyN7rvGE3X6cb0beVimM2SyoIUxwXcpu
PNSTJ5TN55EddIZUBSKg3R+4dgRdZbfw/oVrTJFX2lzX0QOeIot/X7UQVAlfE2TM
+GHtcVAbBFOwzw+zyhduXlBhhupMmPDzFX8r9iimFFrZeRLpsL7PJvz1SnlIUWCV
HGqfRdAyeZInVypIgwIRXqI+z3TnVdef2q26RqhPPykFkp6Nx+sfXjvQUrJp7Zs9
Ks3bPYH/3VLOC6hyW00IVfgVoM7qL+VbIB9QdHMytJCKKZNAn3mc
-----END CERTIFICATE-----";


    public static IPrivateKey GetPrivateKey()
    {
        var data = Convert.FromBase64String(PrivateKey);
        var keyFactory = KeyFactory.GetInstance("RSA");
        return keyFactory.GeneratePrivate(new PKCS8EncodedKeySpec(data));
    }

}
