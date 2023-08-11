using System;
using UnityEngine;

public struct Identity : IEquatable<Identity>
{
    public Identity(string nickName, string userName, string id, string chatColorHex, bool fake = false)
    {
        NickName = nickName;
        UserName = userName;
        ID = id;
        Fake = fake;

        ChatColor = Color.white;
        ColorUtility.TryParseHtmlString(chatColorHex, out ChatColor);
    }

    public bool IsValid
    {
        get
        {
            return !Fake && !string.IsNullOrEmpty(ID);
        }
    }

    public readonly string NickName;
    public readonly string UserName;
    public readonly string ID;
    public readonly bool Fake;
    public readonly Color ChatColor;

    public override string ToString()
    {
        if (string.IsNullOrEmpty(ID))
        {
            return "<Unknown>";
        }

        if (NickName.Equals(UserName, StringComparison.InvariantCultureIgnoreCase))
        {
            return $"{NickName} [{ID}]";
        }

        return $"{NickName} ({UserName}) [{ID}]";
    }

    public override bool Equals(object obj)
    {
        if (obj is Identity other)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                return ID.Equals(other.ID);
            }

            return string.IsNullOrEmpty(other.ID);
        }

        return false;
    }

    public bool Equals(Identity other)
    {
        if (!string.IsNullOrEmpty(ID))
        {
            return ID.Equals(other.ID);
        }

        return string.IsNullOrEmpty(other.ID);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public static bool operator ==(Identity lhs, Identity rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Identity lhs, Identity rhs)
    {
        return !(lhs == rhs);
    }
}
