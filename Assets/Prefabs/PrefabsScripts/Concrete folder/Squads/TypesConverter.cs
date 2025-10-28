using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypesConverter
{
    public static float ConvertTransportTypeSpeed(SquadTransport typeTransport,float currentSpeed)
    {
        switch (typeTransport)
        {
            case SquadTransport.ByFoot: return currentSpeed;
            case SquadTransport.Horses: return currentSpeed * 3;
            case SquadTransport.Cars: return currentSpeed * 4;
            case SquadTransport.Tank: return currentSpeed * 2;
            case SquadTransport.Artillery: return currentSpeed * 1;
        }
        return 0;
    }
    public static int ConvertWeaponType(SquadWeapon typeWeapon,int currentAttack)
    {
        switch (typeWeapon)
        {
            case SquadWeapon.MachineHun: return currentAttack * 2;
            case SquadWeapon.Rifle: return currentAttack * 3;
            case SquadWeapon.SniperRifle: return currentAttack * 4;
            case SquadWeapon.Artillery:return 0;
            case SquadWeapon.Tank: return currentAttack * 2;
        }
        return 0;
    }
    public static int ConvertTransportTypeProtection(SquadTransport typeTransport, int currentProtection)
    {
        switch (typeTransport)
        {
            case SquadTransport.ByFoot: return currentProtection;
            case SquadTransport.Horses: return currentProtection;
            case SquadTransport.Cars: return currentProtection * 2;
            case SquadTransport.Tank: return currentProtection * 2;
            case SquadTransport.Artillery:return 0;
        }
        return 0;
    }

    public static Dictionary<SquadWeapon, int> WeaponCost = new Dictionary<SquadWeapon, int> 
    {
        {SquadWeapon.None, 0},
        {SquadWeapon.AssautRifle,2 },
        {SquadWeapon.MachineHun, 3},
        {SquadWeapon.Rifle, 5},
        {SquadWeapon.SniperRifle,8},
        {SquadWeapon.Tank,35},
        {SquadWeapon.Artillery,50 }
    };
    public static Dictionary<SquadTransport, int> TransportCost = new Dictionary<SquadTransport, int>
    {
        {SquadTransport.ByFoot, 0},
        {SquadTransport.Horses, 2 },
        {SquadTransport.Cars, 6 },
        {SquadTransport.Tank, 40},
        {SquadTransport.Artillery,20}
    };

    public static Dictionary<SquadWeapon, string> WeaponDiscription = new Dictionary<SquadWeapon, string>
    {
        {SquadWeapon.None, "Ничего"},
        {SquadWeapon.AssautRifle,"Винтовка" },
        {SquadWeapon.MachineHun, "Пистолет пулемёт"},
        {SquadWeapon.Rifle, "Автомат"},
        {SquadWeapon.SniperRifle,"Снайперская винтовка"},
        {SquadWeapon.Artillery,"Артиллерия" },
        {SquadWeapon.Tank, "Танк"}
    };
    public static Dictionary<SquadTransport, string> TransportDicription = new Dictionary<SquadTransport, string> 
    {
        {SquadTransport.ByFoot, "Пешком"},
        {SquadTransport.Horses, "Лошади"},
        {SquadTransport.Cars, "Машина"},
        {SquadTransport.Tank, "Танк"},
        {SquadTransport.Artillery, "Артиллерия" }
    };
}
