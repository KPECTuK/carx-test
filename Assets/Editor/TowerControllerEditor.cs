using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimpleTowerController))]
// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class SimpleTowerControllerEditor : TowerControllerEditor<SimpleTowerController> { }

[CustomEditor(typeof(CannonTowerController))]
// ReSharper disable once UnusedMember.Global
public class CannonTowerControllerEditor : TowerControllerEditor<CannonTowerController> { }

public abstract class TowerControllerEditor<TController> : Editor where TController : TowerControllerBase
{
	// ReSharper disable once UnusedMember.Global, InconsistentNaming
	public void OnSceneGUI()
	{
		var controller = target as TController;
		if(controller == null)
		{
			return;
		}

		var backup = Handles.color;
		try
		{
			Handles.color = Color.green;
			Handles.DrawWireDisc(controller.transform.position, controller.transform.up, controller.Range);

			foreach(var monster in FindObjectsOfType<MonsterController>().Where(inspecting => !inspecting.Suspended))
			{
				var magnitude = (monster.transform.position - controller.transform.position).sqrMagnitude;
				var reached = magnitude < controller.Range * controller.Range;

				Handles.color = reached
					? Color.red
					: Color.green;
				Handles.DrawLine(monster.transform.position, controller.transform.position);
				Handles.Label(
					monster.transform.position,
					$"dist: {magnitude:F2}:{controller.Range * controller.Range:F2}");
			}

			var driver = controller.Driver as CannonAutoDriver;
			if(driver != null)
			{
				Handles.Label(
					controller.transform.position,
					$"mf: {driver.CurrentPosLerp:F4} tm: {driver.PredictionTime:F3} root: {driver.Roots:F3} rlerp: {driver.RotationLerp:F4} (aiming: {driver.IsAiming})");
			}

		}
		finally
		{
			Handles.color = backup;
		}
	}
}