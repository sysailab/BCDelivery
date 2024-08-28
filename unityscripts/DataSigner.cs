using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Networking;
using Org.BouncyCastle.OpenSsl;
using Newtonsoft.Json.Linq;


public class DataSigner
{
    private RSAParameters privateKey;

    public DataSigner(string privateKeyPem)
    {
        if (string.IsNullOrEmpty(privateKeyPem))
        {
            throw new ArgumentNullException(nameof(privateKeyPem), "PEM key cannot be null or empty");
        }
        privateKey = ConvertPemToRSAParameters(privateKeyPem);
    }

    public string SignData(string data)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(privateKey);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signedBytes = rsa.SignData(dataBytes, new SHA256CryptoServiceProvider());
            return Convert.ToBase64String(signedBytes);
        }
    }

    private RSAParameters ConvertPemToRSAParameters(string pem)
    {
        using (var reader = new StringReader(pem))
        {
            var pemReader = new PemReader(reader);
            var keyObject = pemReader.ReadObject();
            if (keyObject is AsymmetricCipherKeyPair keyPair)
            {
                var privateKey = (RsaPrivateCrtKeyParameters)keyPair.Private;
                return DotNetUtilities.ToRSAParameters(privateKey);
            }
            else if (keyObject is RsaPrivateCrtKeyParameters rsaPrivateKey)
            {
                return DotNetUtilities.ToRSAParameters(rsaPrivateKey);
            }
            else
            {
                throw new InvalidCastException("The provided PEM file does not contain a valid RSA private key.");
            }
        }
    }
}

