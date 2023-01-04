IMyRemoteControl RemCon;
List<IMyGyro> gyroList;
public Program()
{
gyroList = new List<MyGyro>();
GridTerminalSystem.GetBlockWithName(«RemCon») as IMyRemoteControl;
Runtime.UpdateFrequence = UpdateFrequence.Update1;
}public void Main()
{
Vector3D GravityVector = RemCon.GetNaturalGravity();
Vector3D GravNorm = Vector3D.Normalize(GravityVector);
float RollInput = (float)GravNorm.Dot(RemCon.WorldMatrix.Left);
float PitchInput = -(float)GravNorm.Dot(RemCon.WorldMatrix.Forward);
float YawInput = RemCon.RotationIndicator.Y;foreach (IMyGyro gyro in gyroList)
{
 

gyro.GyroOverride = true;
gyro.Yaw = YawInput;
gyro.Roll = RollInput;
gyro.Pitch = PitchInput;
}
}