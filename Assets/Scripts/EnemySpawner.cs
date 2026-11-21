using UnityEngine;
using UnityEngine.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _numberOfEnemiesToSpawn;
    [SerializeField] private float _spawnRadius = 15f;
    [SerializeField] private Transform _playerTransform;

    [Header("Enemy Stuff")]
    [SerializeField] private float DistanceThreshold = 15f;
    [SerializeField] private float _moveSpeed = 2.5f;

    private bool _isDone;
    private List<GameObject> _enemiesList = new List<GameObject>();

    private TransformAccessArray _accessArray;
    private Transform[] _enemyTransforms;

    private void Awake() {
        _enemyTransforms = new Transform[_numberOfEnemiesToSpawn];
        for(int i = 0; i < _numberOfEnemiesToSpawn; ++i) {
            UnityEngine.Vector2 spawnPosition = UnityEngine.Random.insideUnitCircle * _spawnRadius;
            GameObject enemy = Instantiate(_enemyPrefab, spawnPosition, UnityEngine.Quaternion.identity);
            _enemiesList.Add(enemy);
            _enemyTransforms[i] = enemy.transform;

        }
        _accessArray = new TransformAccessArray(_enemyTransforms);

        _isDone = true;
    }

    private void Update() {
        if(_isDone) {
            ControlEnemyJob job = new ControlEnemyJob() {
                playerPos = _playerTransform.position,
                deltaTime = Time.deltaTime,
                distanceThreshold = DistanceThreshold,
                moveSpeed = _moveSpeed,
            };
            JobHandle jobHandle = job.Schedule(_accessArray);
            jobHandle.Complete();
            /*for(int i = 0; i < _enemiesList.Count; ++i) {
                UnityEngine.Vector2 direction = (_playerTransform.position -_enemiesList[i].transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg - 90f;
                UnityEngine.Quaternion lookRotation = UnityEngine.Quaternion.AngleAxis(angle, UnityEngine.Vector3.forward);
                _enemiesList[i].transform.rotation = lookRotation;
                
                float distance = UnityEngine.Vector2.Distance(_playerTransform.position, _enemiesList[i].transform.position);
                if(distance < DistanceThreshold) {
                    _enemiesList[i].transform.position += -(UnityEngine.Vector3)direction*Time.deltaTime*_moveSpeed;
                }
            }*/
        }
    }

    public struct ControlEnemyJob: IJobParallelForTransform {
        [ReadOnly] public float3 playerPos;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float distanceThreshold;
        [ReadOnly] public float moveSpeed;
        public void Execute(int index, TransformAccess transform) {
            float3 direction = math.normalize(playerPos-(float3)transform.position);
            float angle = math.atan2(direction.y,direction.x);
            angle-=math.radians(90f);
            quaternion lookRotation = quaternion.AxisAngle(new float3(0,0,1),angle);
            transform.rotation = lookRotation;

            float distance = math.distance(playerPos, transform.position);
            float3 newPosition = (float3)transform.position-direction*deltaTime*moveSpeed;
            float3 maskedPosition = math.select(transform.position, newPosition, distance < distanceThreshold);
            transform.position = maskedPosition;
        }
    }
}
