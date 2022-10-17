using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim.VisualEffects
{
    public class Subeffecter_SprayerFixed : SubEffecter
    {
        public Subeffecter_SprayerFixed(SubEffecterDef def, Effecter parent) : base(def, parent)
        {
        }

        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
        {
	        MakeMote(A, B);
        }

        protected void MakeMote(TargetInfo A, TargetInfo B)
		{
			Vector3 vector = Vector3.zero;
			switch (this.def.spawnLocType)
			{
				case MoteSpawnLocType.OnSource:
					vector = A.CenterVector3;
					break;
				case MoteSpawnLocType.BetweenPositions:
					{
						Vector3 vector2 = A.HasThing ? A.Thing.DrawPos : A.Cell.ToVector3Shifted();
						Vector3 vector3 = B.HasThing ? B.Thing.DrawPos : B.Cell.ToVector3Shifted();
						if (A.HasThing && !A.Thing.Spawned)
						{
							vector = vector3;
						}
						else if (B.HasThing && !B.Thing.Spawned)
						{
							vector = vector2;
						}
						else
						{
							vector = vector2 * this.def.positionLerpFactor + vector3 * (1f - this.def.positionLerpFactor);
						}
						break;
					}
				case MoteSpawnLocType.BetweenTouchingCells:
					vector = A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f;
					break;
				case MoteSpawnLocType.RandomCellOnTarget:
					{
						CellRect cellRect;
						if (B.HasThing)
						{
							cellRect = B.Thing.OccupiedRect();
						}
						else
						{
							cellRect = CellRect.CenteredOn(B.Cell, 0);
						}
						vector = cellRect.RandomCell.ToVector3Shifted();
						break;
					}
				case MoteSpawnLocType.OnTarget:
					vector = B.CenterVector3;
					break;
			}
			if (this.parent != null)
			{
				Rand.PushState(this.parent.GetHashCode());
				if (A.CenterVector3 != B.CenterVector3)
				{
					vector += (B.CenterVector3 - A.CenterVector3).normalized * this.parent.def.offsetTowardsTarget.RandomInRange;
				}
				vector += Gen.RandomHorizontalVector(this.parent.def.positionRadius) + this.parent.offset;
				Rand.PopState();
			}
			Map map = A.Map ?? B.Map;
			float num;
			if (this.def.absoluteAngle)
			{
				num = 0f;
			}
			else if (this.def.useTargetAInitialRotation && A.HasThing)
			{
				num = A.Thing.Rotation.AsAngle;
			}
			else if (this.def.useTargetBInitialRotation && B.HasThing)
			{
				num = B.Thing.Rotation.AsAngle;
			}
			else
			{
				num = (B.Cell - A.Cell).AngleFlat;
			}
			float num2 = (this.parent != null) ? this.parent.scale : 1f;
			if (map != null && vector.ShouldSpawnMotesAt(map))
			{
				int randomInRange = this.def.burstCount.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					Vector3 vector4 = vector + this.def.positionOffset * num2 + Gen.RandomHorizontalVector(this.def.positionRadius) * num2;
					if (this.def.moteDef != null)
					{
						Mote mote = (Mote)ThingMaker.MakeThing(this.def.moteDef, null);
						GenSpawn.Spawn(mote, vector.ToIntVec3(), map, WipeMode.Vanish);
						mote.Scale = this.def.scale.RandomInRange * num2;
						mote.exactPosition = vector4;
						mote.rotationRate = this.def.rotationRate.RandomInRange;
						mote.exactRotation = this.def.rotation.RandomInRange + num;
						mote.instanceColor = base.EffectiveColor;
						MoteThrown moteThrown = mote as MoteThrown;
						if (moteThrown != null)
						{
							moteThrown.airTimeLeft = this.def.airTime.RandomInRange;
							moteThrown.SetVelocity(this.def.angle.RandomInRange + num, this.def.speed.RandomInRange);
						}
					}
					else if (this.def.fleckDef != null)
					{
						float velocityAngle = this.def.fleckUsesAngleForVelocity ? (this.def.angle.RandomInRange + num) : 0f;
						map.flecks.CreateFleck(new FleckCreationData
						{
							def = this.def.fleckDef,
							scale = this.def.scale.RandomInRange * num2,
							spawnPosition = vector4,
							rotationRate = this.def.rotationRate.RandomInRange,
							rotation = this.def.rotation.RandomInRange + num,
							instanceColor = new Color?(base.EffectiveColor),
							velocitySpeed = this.def.speed.RandomInRange,
							velocityAngle = velocityAngle,
							
                        });
					}
				}
			}
		}
	}
}
