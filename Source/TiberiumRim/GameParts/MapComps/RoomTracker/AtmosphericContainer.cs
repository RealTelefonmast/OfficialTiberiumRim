using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AtmosphericContainer : IContainerHolder
    {
        private NetworkContainer container;
        private RoomComponent parentComp;

        public Thing Thing => null;
        public NetworkContainer Container => container;

        public string ContainerTitle => $"Room({parentComp?.Room?.ID.ToString() ?? "NOROOM"}) Container";

        public int Value => (int)Container.TotalStored;
        public NetworkValueStack ValueStack => Container.ValueStack;

        public float Saturation => Container.StoredPercent;
        public bool FullySaturated => Container.CapacityFull;

        public AtmosphericContainer(RoomComponent parent)
        {
            parentComp = parent;
            container = new NetworkContainer(this, new ContainerProperties()
            {
                doExplosion = false,
                dropContents = false,
                leaveContainer = false,
                maxStorage = 0
            }, new List<NetworkValueDef>()
            {
                TiberiumDefOf.TibPollution
            });
        }

        public void Notify_ContainerFull() { }

        public void TransferAllTo(AtmosphericContainer other)
        {
            foreach (var type in Container.AllStoredTypes)
            {
                Container.TryTransferTo(other.Container, type, Container.ValueForType(type));
            }
        }

        public void EqualizeWith(AtmosphericContainer other, int flowAmount)
        {
            float partValue = (float)flowAmount / Container.AllStoredTypes.Count;
            foreach (var type in Container.AllStoredTypes)
            {
                Container.TryTransferTo(other.Container, type, partValue);
            }
        }

        //Set New Data When RoomComp changes (important with Map-Rooms)
        public void RegenerateData(RoomComponent parent, int roomCells)
        {
            this.parentComp = parent;
            Container.Data_ChangeCapacity(roomCells * AtmosphericMapInfo.CELL_CAPACITY);

        }

        public void Data_SetInfo(Dictionary<NetworkValueDef, int> data)
        {
            Log.Message("Setting new data...");
            Container.Clear();
            foreach (var value in data)
            {
                Log.Message($"Adding {value.Key}: {value.Value}");
                Container.TryAddValue(value.Key, value.Value, out _);
            }
        }

        public bool TryEqualize(AtmosphericContainer other, float passPercent, out bool toOther)
        {
            toOther = false;
            var diff = (Saturation - other.Saturation);
            var diffAbs = Math.Abs(diff);
            if (!(diffAbs > 0.01f)) return false;

            toOther = diff > 0;
            var sendingContainer = toOther ? Container : other.Container;
            var receivingContainer = toOther ? other.Container : Container;
            var partCount = sendingContainer.ValueStack.networkValues.Length;
            //flowAmount = AtmosphericMapInfo.CELL_CAPACITY * diffAbs * passPercent;
            sendingContainer.TryTransferTo(receivingContainer, (AtmosphericMapInfo.CELL_CAPACITY * diffAbs * passPercent) / partCount);
            return true;
        }
    }

    /*public class PollutionContainer 
    {
        private int pollutionInt = 0;

        //public int ContainerCells { get; private set; } = 0;
        public int TotalCapacity { get; private set; } = 0;

        public float Saturation => Pollution / (float)TotalCapacity;
        public bool FullySaturated => Pollution >= TotalCapacity;

        public PollutionContainer()
        {
        }

        public int Pollution
        {
            get => pollutionInt;
            set => pollutionInt = value;
        }

        public bool TryAddValue(int wantedValue, out int actualValue)
        {
            //If we add more than we can contain, we have an excess weight
            int excessValue = (int)Mathf.Clamp((Pollution + wantedValue) - TotalCapacity, 0, float.MaxValue);
            //The actual added weight is the wanted weight minus the excess
            actualValue = wantedValue - excessValue;

            //If the container is full, or doesnt accept the type, we dont add anything
            if (FullySaturated)
            {
                //Notify_Full();
                return false;
            }

            //If the weight type is already stored, add to it, if not, make a new entry
            pollutionInt += actualValue;

            //If this adds the last drop, notify full
            /*
            if (FullySaturated)
                Notify_Full();
            #1#

            //Notify_AddedValue(valueType, actualValue);
            return true;
        }

        public bool TryRemoveValue(int wantedValue, out int actualValue)
        {
            //Attempt to remove a certain weight from the container
            actualValue = 0;
            var value = Pollution;
            if (value > 0)
            {
                if (value >= wantedValue)
                {
                    //If we have stored more than we need to pay, remove the wanted weight
                    pollutionInt -= wantedValue;
                    actualValue = wantedValue;
                }
                else
                {
                    //If not enough stored to "pay" the wanted weight, remove the existing weight and set actual removed weight to removed weight 
                    pollutionInt = 0;
                    actualValue = value;
                }
            }

            //Notify_RemovedValue(valueType, actualValue);
            return actualValue == wantedValue;
        }

        //Set New Data When RoomComp changes (important with Map-Rooms)
        public void RegenerateData(int roomCells)
        {
            TotalCapacity = roomCells * AtmosphericMapInfo.CELL_CAPACITY;

        }
    }*/

}
