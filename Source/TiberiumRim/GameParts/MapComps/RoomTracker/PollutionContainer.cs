using UnityEngine;

namespace TiberiumRim
{
    public class PollutionContainer 
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
            */

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
            TotalCapacity = roomCells * TiberiumPollutionMapInfo.CELL_CAPACITY;

        }
    }
}
