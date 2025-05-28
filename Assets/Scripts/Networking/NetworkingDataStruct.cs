/* Original author name: Adam Turcotte
 * Creation date: 2025/04/29
 * Goal: Struc to transform class for network
 * Modification listing:
 *-
 */

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

namespace Reseau
{
 
    public struct PlayerData : INetworkSerializable
    {
        public Vector3 position;
        public Vector3 PlayerVelocity;
        public Vector2 GunPosition;
        public float gunScale;
        public ulong id;

        public PlayerData(ulong PlayerId, Vector3 PlayerBody, Vector3 PlayerSpeed, Vector3 GunBody, Vector3 GunScale)
        {
            id = PlayerId;
            position = PlayerBody;
            PlayerVelocity = PlayerSpeed;
            GunPosition = GunBody;
            gunScale = GunScale.y;
        }

        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[4 * 7 + sizeof(ulong)];
            Buffer.BlockCopy(BitConverter.GetBytes(id), 0, buffer, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, buffer, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, buffer, 12, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(PlayerVelocity.x), 0, buffer, 16, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(PlayerVelocity.y), 0, buffer, 20, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(GunPosition.x), 0, buffer, 24, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(GunPosition.y), 0, buffer, 28, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(gunScale), 0, buffer, 32, 4);
            return buffer;
        }

        public PlayerData FromByteArray(byte[] data)
        {
            PlayerData pd = new PlayerData();
            pd.id = BitConverter.ToUInt64(data, 0);
            pd.position = new Vector3(BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12),0);
            pd.PlayerVelocity = new Vector3(BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20), 0);
            pd.GunPosition = new Vector2(BitConverter.ToSingle(data, 24), BitConverter.ToSingle(data, 28));
            pd.gunScale = BitConverter.ToSingle(data, 32);
            return pd;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref PlayerVelocity);
            serializer.SerializeValue(ref GunPosition);
            serializer.SerializeValue(ref gunScale);
            serializer.SerializeValue(ref id);
        }
    }

    public struct ShootData : INetworkSerializable
    {
        public ulong ID;
        public Vector3 Position;
        public Quaternion GunRotation;

        public ShootData(ulong id, Vector3 position, Quaternion rotation)
        {
            ID = id;
            Position = position;
            GunRotation = rotation;
        }

        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[sizeof(ulong) + sizeof(float) * 7];
            Buffer.BlockCopy(BitConverter.GetBytes(ID), 0, buffer, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(Position.x), 0, buffer, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Position.y), 0, buffer, 12, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Position.z), 0, buffer, 16, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(GunRotation.x), 0, buffer, 20, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(GunRotation.y), 0, buffer, 24, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(GunRotation.z), 0, buffer, 28, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(GunRotation.w), 0, buffer, 32, 4);
            return buffer;
        }

        public static ShootData FromByteArray(byte[] data)
        {
            ShootData sd;
            sd.ID = BitConverter.ToUInt64(data, 0);
            sd.Position = new Vector3(
                BitConverter.ToSingle(data, 8),
                BitConverter.ToSingle(data, 12),
                BitConverter.ToSingle(data, 16));

            sd.GunRotation = new Quaternion(
                BitConverter.ToSingle(data, 20),
                BitConverter.ToSingle(data, 24),
                BitConverter.ToSingle(data, 28),
                BitConverter.ToSingle(data, 32));

            return sd;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref GunRotation);
            serializer.SerializeValue(ref ID);
        }
    }

    public struct GrappleData : INetworkSerializable
    {
        public ulong ID;
        public Vector2 Direction;

        public GrappleData(ulong id, Vector2 direction )
        {
            ID = id;
            Direction = direction;
        }

        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[sizeof(ulong) + sizeof(float) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(ID), 0, buffer, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(Direction.x), 0, buffer, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Direction.y), 0, buffer, 12, 4);
            return buffer;
        }

        public static GrappleData FromByteArray(byte[] data)
        {
            GrappleData gd = new GrappleData
            {
                ID = BitConverter.ToUInt64(data, 0),
                Direction = new Vector2(BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12)),
            };
            return gd;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Direction);
            serializer.SerializeValue(ref ID);
        }
    }

    public struct RsaKeyData
    {
        private byte[] KeyN;
        private byte[] KeyOther;

        public RsaKeyData(RsaKeyData data)
        {
            KeyN = data.KeyN;
            KeyOther = data.KeyOther;
        }

        [Obsolete]
        public RsaKeyData(List<byte[]> privateKey)
        {
            KeyN = privateKey[0];
            KeyOther = privateKey[1];
        }

        public RsaKeyData(byte[] keyN, byte[] keyOther)
        {
            KeyN = keyN;
            KeyOther = keyOther;
        }

        public byte[] ToByteArray()
        {
            var key1Len = KeyN.Length;
            var key2Len = KeyOther.Length;
            var buffer = new byte[4 + key1Len + key2Len];

            Buffer.BlockCopy(BitConverter.GetBytes(key1Len), 0, buffer, 0, 4);
            Buffer.BlockCopy(KeyN, 0, buffer, 4, key1Len);
            Buffer.BlockCopy(KeyOther, 0, buffer, 4 + key1Len, key2Len);
            return buffer;
        }

        public static RsaKeyData FromByteArray(byte[] data)
        {
            RsaKeyData kd = new();
            var key1Len = BitConverter.ToInt32(data, 0);
            kd.KeyN = new byte[key1Len];
            kd.KeyOther = new byte[data.Length - 4 - key1Len];

            Buffer.BlockCopy(data, 4, kd.KeyN, 0, key1Len);
            Buffer.BlockCopy(data, 4 + key1Len, kd.KeyOther, 0, kd.KeyOther.Length);

            return kd;
        }

        public byte[] getN()
        {
            return KeyN;
        }

        public byte[] getOther()
        {
            return KeyOther;
        }
    }

    public struct AesKeyData
    {
        private byte[] key;
        private byte[] iv;


        public AesKeyData(Aes aes)
        {
            key = aes.Key;
            iv = aes.IV;
        }

        public byte[] ToByteArray()
        {
            int keyLen = key.Length;
            int ivLen = iv.Length;
            var buffer = new byte[4 + keyLen + ivLen];

            Buffer.BlockCopy(BitConverter.GetBytes(keyLen), 0, buffer, 0, 4);
            Buffer.BlockCopy(key, 0, buffer, 4, keyLen);
            Buffer.BlockCopy(iv, 0, buffer, 4 + keyLen, ivLen);
            return buffer;
        }

        public static AesKeyData FromByteArray(byte[] data)
        {
            AesKeyData kd = new();
            int keyLen = BitConverter.ToInt32(data, 0);
            kd.key = new byte[keyLen];
            kd.iv = new byte[data.Length - 4 - keyLen];

            Buffer.BlockCopy(data, 4, kd.key, 0, keyLen);
            Buffer.BlockCopy(data, 4 + keyLen, kd.iv, 0, kd.iv.Length);
            return kd;
        }

        public byte[] getKey()
        {
            return key;
        }

        public byte[] getIV()
        {
            return iv;
        }
    }
}