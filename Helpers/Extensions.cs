/**
 *   Copyright (C) 2021 okaygo, OneXDeveloper
 *
 *   https://github.com/misterokaygo/D2RAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/
using System;
using D2RAssist.Types;
using static D2RAssist.Types.Game;

namespace D2RAssist.Helpers
{
    public static class Extensions
    {
        public static Area toArea(this String areaIdString) {
            return (Area)Convert.ToInt16 (areaIdString);
        }

        public static bool IsTown(this Area area) =>
            area == Area.RogueEncampment || area == Area.LutGholein || area == Area.KurastDocks ||
            area == Area.ThePandemoniumFortress || area == Area.Harrogath;

        public static bool IsForward(this Area area) {
            switch (Globals.CurrentGameData.AreaId) {
                case Area.BlackMarsh:
                    if (area == Area.ForgottenTower || area == Area.TamoeHighland)
                        return true;
                    break;
                case Area.TowerCellarLevel1:
                    if (area == Area.TowerCellarLevel2)
                        return true;
                    break;
                case Area.TowerCellarLevel2:
                    if (area == Area.TowerCellarLevel3)
                        return true;
                    break;
                case Area.TowerCellarLevel3:
                    if (area == Area.TowerCellarLevel4)
                        return true;
                    break;
                case Area.TowerCellarLevel4:
                    if (area == Area.TowerCellarLevel5)
                        return true;
                    break;
                case Area.CatacombsLevel1:
                    if (area == Area.CatacombsLevel2)
                        return true;
                    break;
                case Area.CatacombsLevel2:
                    if (area == Area.CatacombsLevel3)
                        return true;
                    break;
                case Area.CatacombsLevel3:
                    if (area == Area.CatacombsLevel4)
                        return true;
                    break;
                case Area.FarOasis:
                    if (area == Area.LostCity || area == Area.MaggotLairLevel1)
                        return true;
                    break;
                case Area.LostCity:
                    if (area == Area.AncientTunnels || area == Area.ValleyOfSnakes)
                        return true;
                    break;
                case Area.CanyonOfTheMagi:
                    if (area == Area.TalRashasTomb1 || area == Area.TalRashasTomb2 || area == Area.TalRashasTomb3 ||
                        area == Area.TalRashasTomb4 || area == Area.TalRashasTomb5 || area == Area.TalRashasTomb6 ||
                        area == Area.TalRashasTomb7)
                        return true;
                    break;
                case Area.DuranceOfHateLevel1:
                    if (area == Area.DuranceOfHateLevel2)
                        return true;
                    break;
                case Area.DuranceOfHateLevel2:
                    if (area == Area.DuranceOfHateLevel3)
                        return true;
                    break;
                case Area.CrystallinePassage:
                    if (area == Area.FrozenRiver || area == Area.GlacialTrail)
                        return true;
                    break;
                case Area.GlacialTrail:
                    if (area == Area.FrozenTundra)
                        return true;
                    break;
                case Area.HallsOfPain:
                    if (area == Area.HallsOfVaught)
                        return true;
                    break;
                case Area.TheAncientsWay:
                    if (area == Area.ArreatSummit)
                        return true;
                    break;
                case Area.TheWorldStoneKeepLevel1:
                    if (area == Area.TheWorldStoneKeepLevel2)
                        return true;
                    break;
                case Area.TheWorldStoneKeepLevel2:
                    if (area == Area.TheWorldStoneKeepLevel3)
                        return true;
                    break;
                case Area.TheWorldStoneKeepLevel3:
                    if (area == Area.ThroneOfDestruction)
                        return true;
                    break;
            }
            return false;
        }
    }
}
