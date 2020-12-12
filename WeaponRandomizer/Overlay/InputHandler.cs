using Gear;
using Player;
using SNetwork;
using System;
using UnityEngine;
using WeaponRandomizer.WeaponRandomizer;

namespace WeaponRandomizer.Overlay
{
    class InputHandler : MonoBehaviour
    {
        private readonly WeaponRandomizer.WeaponRandomizer _weaponRandomizer;
        private string _normalName;

        private DateTime _nextPeriodInterval;

        private bool _isActivated = false;
        private bool _wasInLevel;

        /// <summary>
        /// Reads all Weapons
        /// </summary>
        public InputHandler(IntPtr intPtr) : base(intPtr)
        {
            _normalName = SteamManager.LocalPlayerName;
            _weaponRandomizer = new WeaponRandomizer.WeaponRandomizer();
        }

        public string NormalName
        {
            get
            {
                if (string.IsNullOrEmpty(_normalName))
                {
                    _normalName = SteamManager.LocalPlayerName;
                }

                return _normalName;
            }
        }

        public bool WasInLevel
        {
            get => _wasInLevel;
            set
            {
                if (!value)
                {
                    _weaponRandomizer.NewExpeditionStarted();
                }

                _wasInLevel = value;
            }
        }

        public void Update()
        {
            if ((byte)GameStateManager.CurrentStateName < 5)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.G))
                {
                    if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.N) && Input.GetKey(KeyCode.G))
                    {
                        //To prevent Activating / Deactivating it while in Game
                        if (PlayerManager.PlayerAgentsInLevel.Count == 0)
                        {
                            Console.WriteLine($"WeaponRandomizer = {(!_isActivated ? "Turned ON" : "Turned OFF")}");
                            if (_isActivated)
                            {
                                ModIdentifier.ModdingName = _normalName;
                                _isActivated = false;
                            }
                            else
                            {
                                _isActivated = true;
                                ModIdentifier.ModdingName = $"[Modded] {_normalName}";
                            }
                        }
                    }
                }
            }

            if(Input.GetKeyDown(KeyCode.X))
            {
                WeaponRandomizer.WeaponRandomizer.LocalInventory = UnityEngine.Object.FindObjectOfType<PlayerInventoryLocal>();
                _weaponRandomizer.RandomizeCurrentWeapons();
            }

            //Some Timer imitation, because the Normal System.Timer thing does not work
            if (_nextPeriodInterval < DateTime.Now)
            {
                if (_isActivated && GameStateManager.CurrentStateName is eGameStateName.InLevel)
                {
                    if (PlayerManager.PlayerAgentsInLevel.Count != 0)
                    {
                        if (!WasInLevel)
                        {
                            WeaponRandomizer.WeaponRandomizer.LocalInventory = UnityEngine.Object.FindObjectOfType<PlayerInventoryLocal>();
                            WasInLevel = true;
                        }

                        switch (_weaponRandomizer.ZoneSwitched())
                        {
                            case ActionRequired.NewZoneEntered:
                                _weaponRandomizer.RandomizeCurrentWeapons();
                                break;
                            case ActionRequired.ExploredZoneEntered:
                                _weaponRandomizer.NewZoneEntered();
                                break;
                        }
                    }
                }
                else if (WasInLevel)
                {
                    WeaponRandomizer.WeaponRandomizer.LocalInventory = null;
                    WasInLevel = false;
                }

                _nextPeriodInterval = DateTime.Now.AddMilliseconds(500);
            }
        }
    }
}
