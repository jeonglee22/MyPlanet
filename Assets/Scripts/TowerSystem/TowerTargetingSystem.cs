using UnityEngine;

public enum RangeType
{
    Short,
    Mid,
    Long
}

public class TowerTargetingSystem : MonoBehaviour
{//updateTarget->GetEnemiesInRange(consider targetPriority) -> Return currentTarget -> driven atk sys

    RangeType rangeType;
    BaseTargetPriority targetPriority;
    Transform randomTowerCenter; //assign tower object
    ITargetable currentTarget; //current target cashing
    float scanInterval; // Multiple Interval Scan System

    private void Start()
    {
        ScanEnemies();
    }

    private void ScanEnemies()
    {
    }

    private void UpdateTarget() //strategy -> target choice
    { //callback in Tower Manager !

        //ITargetable filtering -> ITargetable SelectPriority
        
        //매 프레임 호출 + 공격 중 탐색 중지

    }
    private void GetEnemyInRange() //enemy check -> collect enemy List
    {
        //currentTarget Cashing -> atk system driving !
        //Refactor:ObjectPool (grid, quadTree)
    }

    private void HasValidTarget() // valid check
    {

    }

    private void ReleaseTarget()
    {

    }


}
