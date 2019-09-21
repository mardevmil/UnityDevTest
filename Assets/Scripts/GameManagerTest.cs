using UnityEngine;
using mardevmil.Core;
using System.Collections.Generic;

public class GameManagerTest : MonoBehaviour
{
    [SerializeField]
    private Transform _velocityObject;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private GameObject _testBlock;
    [SerializeField]
    private GameObject _testGround;

    private ObjectPooler _poolBlocks;
    private ObjectPooler _poolGrounds;

    private Vector3 _groundScale = new Vector3(4f, 1f, 15f);
    private Vector3 _groundPos = new Vector3(5f, 5f, 5f);

    private List<GameObject> _grounds = new List<GameObject>(10);

    private bool debug = false;

    void Start()
    {
        //var index = TimerManager.AddTimer(1f, false);
        //TimerManager.Timers[index].onTimerFinished += Teeest;
        //TimerManager.Timers[index].IsActive = true;
        EventManager.testVoidEvent += OnTestVoidEvent;
        _poolBlocks = new ObjectPooler(_testBlock, 20);
        _poolGrounds = new ObjectPooler(_testGround, 10);
    }
    
    public void Update()
    {
        //TimerManager.Update(Time.deltaTime);
        if(Input.GetKeyUp(KeyCode.G))
        {
            var ground = _poolGrounds.Get();
            if(ground)
            {
                EventManager.testVoidEvent();
                ground.transform.localScale = _groundScale;
                ground.transform.position = Vector3.zero;
                _grounds.Add(ground);
            }
        }
        if (Input.GetKeyUp(KeyCode.H))
        {
            if(_grounds.Count > 0)
            {
                var index = Random.Range(0, _grounds.Count);
                _poolGrounds.Release(_grounds[index]);
                _grounds.RemoveAt(index);
            }
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            var rigbody = _velocityObject.GetComponent<Rigidbody>();
            if(rigbody != null)
            {
                rigbody.velocity = PhysicMath.CalculateVelocity(_velocityObject, _target, 60f);
                debug = true;
            }

        }
        if(debug)
            Debug.LogError("+++ " + _target.position + " / " + _velocityObject.position + " *** " + Vector3.Distance(_target.position, _velocityObject.position));

        //var direction = target.position - velocityObject.position;
        //Debug.DrawLine(_velocityObject.position, direction, Color.yellow);

    }

    private void OnDestroy()
    {
        EventManager.testVoidEvent -= OnTestVoidEvent;
        //TimerManager.Purge();
        //DelayManager.Purge();
    }

    private void Teeest()
    {
        //Debug.LogError("+++ Teeest");
    }

    private void OnTestVoidEvent()
    {
        Debug.LogError("+++ OnTestVoidEvent");
    }
}
