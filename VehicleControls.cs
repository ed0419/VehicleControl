using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VehicleControls
{
    public class VehicleControls : BaseScript
    {
        private static string ERROR = "~r~錯誤: ";
        private static string ERROR_NOCAR = ERROR + "你不在車輛內或者你不擁有這輛車";

        private Vehicle savedVehicle;

        private void AddEngineItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("引擎開關");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    ToggleEngine(car);
                }
                else if (savedVehicle != null)
                {
                    ToggleEngine(savedVehicle);
                }
            };
        }

        private void ToggleEngine(Vehicle car)
        {
            if (car.IsEngineRunning)
            {
                Screen.ShowNotification("引擎 ~r~熄火~w~.");
                car.IsDriveable = false;
                car.IsEngineRunning = false;
            }
            else
            {
                Screen.ShowNotification("引擎 ~g~發動~w~.");
                car.IsDriveable = true;
                car.IsEngineRunning = true;
            }
        }

        private void AddDoorLockItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("車門開關", "注意:這項操作將自動儲存此車輛");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == newItem)
                {
                    Vehicle car = LocalPlayer.Character.CurrentVehicle;

                    if (car == null
                     && savedVehicle == null)
                    {
                        Screen.ShowNotification(ERROR_NOCAR);
                        return;
                    }

                    if (car != null)
                    {
                        LockDoor(car);
                        SaveVehicle(car);
                    }
                    else
                    {
                        LockDoor(savedVehicle);
                    }
                }
            };
        }

        private void LockDoor(Vehicle car)
        {
            bool doorLocked = (Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, car) == 2);

            if (doorLocked)
            {
                Screen.ShowNotification("車門已經 ~g~解鎖了~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 0);
            }
            else
            {
                Screen.ShowNotification("車門已經 ~r~鎖住了~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 2);
            }
        }

        private void AddOpenDoorItem(UIMenu menu)
        {
            List<dynamic> doors = new List<dynamic>
            {
                "左前",
                "右前",
                "左後",
                "右後",
                "引擎蓋",
                "後車箱"
            };
            var newItem = new UIMenuListItem("開關車門", doors, 0);
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                int itemIndex = newItem.Index;
                string doorName = newItem.IndexToItem(itemIndex);
                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    ToggleDoor(car, itemIndex, doorName);
                }
                else
                {
                    ToggleDoor(savedVehicle, itemIndex, doorName);
                }
            };
        }

        private void ToggleDoor(Vehicle car, int index, string doorName)
        {
            bool doorBroken = Function.Call<bool>(Hash.IS_VEHICLE_DOOR_DAMAGED, car, index);
            if (doorBroken)
            {
                Screen.ShowNotification(ERROR + "車門已損毀");
                return;
            }

            float doorAngle = Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, car, index);
            if (doorAngle == 0) // Door is closed
            {
                Screen.ShowNotification(doorName + " 車門已經 ~g~開啟了~w~.");
                Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, car, index, false, false);
            }
            else
            {
                Screen.ShowNotification(doorName + " 車門已經 ~r~關閉了~w~.");
                Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, car, index, false);
            }
        }

        private void AddLockSpeedItem(UIMenu menu)
        {
            List<dynamic> speeds = new List<dynamic>()
            {
                "None"
            };
            for (int i = 30; i < 121; i = i + 10)
            {
                speeds.Add(i + " KM/H");
            }
            UIMenuListItem newItem = new UIMenuListItem("鎖定最高時速", speeds, 0);
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    LockSpeed(car, newItem);
                }
                else
                {
                    LockSpeed(savedVehicle, newItem);
                }
            };
        }

        private void LockSpeed(Vehicle car, UIMenuListItem item)
        {
            string[] itemName = item.IndexToItem(item.Index).Split(' ');
            if (itemName[0] == "None")
            {
                car.MaxSpeed = int.MaxValue;
                Screen.ShowNotification($"車輛限速已被解除");
                return;
            }

            float itemSpeed = float.Parse(itemName[0]) / 3.6f;
            car.MaxSpeed = itemSpeed;
            Screen.ShowNotification($"車輛已被限速在 {itemName[0]} {itemName[1]}.");
        }

        private void AddSaveVehicleItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("儲存車輛");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                SaveVehicle(car);
                Screen.ShowNotification("已成功儲存車輛");
            };
        }

        private void SaveVehicle(Vehicle car)
        {
            if (savedVehicle != null)
            {
                foreach (Blip vehBlip in savedVehicle.AttachedBlips)
                {
                    vehBlip.Alpha = 0;
                }
            }

            Blip blip = car.AttachBlip();
            blip.Sprite = BlipSprite.PersonalVehicleCar;

            savedVehicle = car;
        }

        public VehicleControls()
        {
            MenuPool menuPool = new MenuPool();

            UIMenu menu = new UIMenu("代客修改此", "");
            menuPool.Add(menu);

            AddEngineItem(menu);
            AddDoorLockItem(menu);
            AddOpenDoorItem(menu);
            AddLockSpeedItem(menu);
            AddSaveVehicleItem(menu);

            menu.RefreshIndex();

            Tick += new Func<Task>(async delegate
            {
                await Task.FromResult(0);

                menuPool.ProcessMenus();
                if (Game.IsControlJustReleased(1, Control.InteractionMenu))
                {
                    menu.Visible = !menu.Visible;
                }
            });
        }
    }
}
