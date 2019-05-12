namespace mardevmil.Environment
{
    using UnityEngine;
    using UnityEngine.Events;
    using mardevmil.Core;

    public class DropshipController : MonoBehaviour
    {
        public enum DropshipState
        {
            None,
            Hovering,
            DestructionLanded,
            DestructionHovering,
            Landing,            
            TakeOff,            
            Landed,
            Fly,                       
            Hit,
            Fall
        }

        #region Fields
        [SerializeField, Header(" Door of cargo part")]
        private GameObject _doorSpawnObject;
        [SerializeField, Header("waypoints Parent")]
        private Transform _waypointsParent;
        [SerializeField, Header("Door point - place where cargo part will be attached")]
        private Transform _doorPoint;        
        [Space, Space, SerializeField]
        private Transform _flyingObject;
        [SerializeField]
        private GameObject _deathEffect;        

        [SerializeField]
        private float minSpeed = 5f;
        [SerializeField]
        private bool _landOnDestination = false;
        [SerializeField]
        private bool _patrolMode = true;
        [SerializeField]
        private bool _showWaypoints = false;
        [SerializeField]
        private Transform[] waypoints;        

        [SerializeField, Range(0.1f, 160f)]
        private float _flyingSpeed = 1f;
        [SerializeField, Range(1f, 20f)]
        private float _takeOffSpeed = 13f;
        [SerializeField, Range(1f, 20f)]
        private float _slowDownSpeed = 9f;
        [SerializeField, Range(0.1f, 9f)]
        private float _rotateSpeed = 1f;
        [SerializeField, Range(1f, 60f)]
        private float _reloadTime = 5f;
        [SerializeField, Range(0.1f, 60f)]
        private float _uloadDelayTime = 1f;
        [SerializeField, Range(3.5f, 60f)]
        private float _waitingTimeAfterUnload = 3.5f;
        [SerializeField, Range(0.5f, 0.95f), Tooltip("Percent traveled path until start slowing down")]
        private float _percentPathUntilSlowdown = 0.8f;        

        [Space]
        [Space]
        public UnityEvent arrivedOnDestination;
        public UnityEvent cargoUnloaded;
        public UnityEvent returnedOnStartPostion;
        public UnityEvent destroyed;
        public UnityEvent flewReloaded;
        [Space, Header("*** Animation events ***"), Space]
        public UnityEvent takeOffStart;
        public UnityEvent takeOffFinished;
        public UnityEvent landingStart;
        public UnityEvent landingFinished;
        public UnityEvent doorOpened;
        public UnityEvent doorClosed;
        public UnityEvent doorStartOpening;
        public UnityEvent doorStartClosing;

        //private EnemySpawnManager _spawner;
        private Animator _doorAnimator;
        //private Destructible _destructible;
        private Animator _animator;
        private bool _flying = false;
        private bool _reverseDirection = false; // rikverc :D

        private Vector3 _destinationDirection = Vector3.zero;
        private Vector3 _calculatedDirection = Vector3.zero;

        private float _currentFlyingSpeed = 0f;
        private float _currentRotateSpeed = 0f;
        private Vector3 _destinationPoint = Vector3.zero;
        private int _currentWaypointIndex = 0;
        private Quaternion _tmpRot = Quaternion.identity;
        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startRot = Quaternion.identity;
        private float _destinationDistance = 0f;
        private bool _slowDown = false;
        private bool _isDead = false;
        private float _partTraveledDistance = 0f;
        private float _traveledDistance = 0f;
        private float _totalDistance = 0f;
        private Vector3 _previousDestinationPoint = Vector3.zero;
        private Rigidbody _flyingObjectRig;        

        private DropshipState _state = DropshipState.None;

        private DropshipState State
        {
            get { return _state; }
            set
            {
                _state = value;
                switch (_state)
                {
                    case DropshipState.Hovering:
                        _animator.SetTrigger("Hovering");
                        break;
                    case DropshipState.DestructionLanded:
                        _animator.SetTrigger("LandedDestroy");
                        break;
                    case DropshipState.DestructionHovering:
                        _animator.SetTrigger("HoveringDestroy");
                        break;
                    case DropshipState.Landing:
                        _animator.SetTrigger("Landing");
                        break;
                    case DropshipState.TakeOff:
                        _animator.SetTrigger("TakeOff");                                                
                        break;
                    case DropshipState.Fly:
                        _animator.SetTrigger("Fly");
                        break;
                    case DropshipState.Hit:
                        _animator.SetTrigger("Hit");
                        break;
                    case DropshipState.Fall:
                        _animator.SetTrigger("Fall");
                        break;
                }
            }
        }
        #endregion

        #region Mono
        void Start()
        {
            _animator = GetComponentInChildren<Animator>(true);
            if(_doorPoint == null || _doorSpawnObject == null || _waypointsParent == null)
            {
                Debug.LogError("For " + gameObject.name + " not defined door spawn object or not defined waypoints parent or door Point is not resolved");
                gameObject.SetActive(false);
                return;                
            }

            if (_showWaypoints)
                ToggleWaypointsVisuals();

            if (_doorPoint.transform.position != _doorSpawnObject.transform.position)
            {
                _doorSpawnObject.transform.position = _doorPoint.transform.position;
                _doorSpawnObject.transform.SetParent(_doorPoint);
            }

            if(waypoints.Length == 0)            
                CollectAndRenameWaypoints();
            
            _doorAnimator = _doorSpawnObject.GetComponentInChildren<Animator>();
            //_spawner = _doorSpawnObject.GetComponentInChildren<EnemySpawnManager>();

            _startPos = _flyingObject.position;
            _startRot = _flyingObject.rotation;

            //if (_destructible == null)
            //    _destructible = GetComponentInChildren<Destructible>();

            _flyingObjectRig = _flyingObject.GetComponent<Rigidbody>();

            //var eventForwarders = transform.GetComponentsInChildren<AnimationEventForwarder>(true);
            //for (int i = 0; i < eventForwarders.Length; i++)           
            //    eventForwarders[i].dropshipController = this;
           
            //_destructible.onDeath.AddListener(OnDeath);
            //_destructible.onDamage.AddListener(OnDamaged);
            //if (_destructible.IsDead)
             //   OnDeath();

            CollectAndAssignDamagableParts();

            //_spawner?.spawnerEndEvent.AddListener(AllEnemiesUnshipped);
            if (waypoints.Length > 0)
            {
                SetDestination(waypoints[0].position);
                _currentWaypointIndex = 0;
            }

            _totalDistance = Vector3.Distance(_flyingObject.position, waypoints[0].position);
            for (int i = 0; i < waypoints.Length - 1; i++)
                _totalDistance += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);

            _currentFlyingSpeed = 0f;
            _currentRotateSpeed = 0f;
            _slowDown = false;
        }

        private void OnDestroy()
        {
            if (_showWaypoints)
                ToggleWaypointsVisuals();

            //_destructible?.onDeath.RemoveAllListeners();
            //_spawner?.spawnerEndEvent.RemoveAllListeners();
        }

        void Update()
        {
            if (_isDead)
            {
                if (State == DropshipState.Fall)
                {
                    if (_flyingObjectRig.velocity.y >= 0f)
                    {
                        //Debug.LogError("+++ GROUNDED " + State);
                        State = DropshipState.DestructionHovering;
                    }
                }
                else if (State == DropshipState.Landed || State == DropshipState.TakeOff)
                {
                    State = DropshipState.DestructionLanded;
                }
                return;
            }

            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.I))
                ResetShip(true);
            if (Input.GetKeyDown(KeyCode.P))
                StartFlying();
            #endif

            if (_flying && _destinationPoint != null)
            {
                SetDestination(_currentWaypointIndex >= 0 ? waypoints[_currentWaypointIndex].position : _startPos);
                Debug.DrawRay(_flyingObject.position, _destinationDirection, Color.yellow);

                if (!TowardLastWaypoint())
                {
                    if (!_slowDown && _currentFlyingSpeed < _flyingSpeed)
                    {
                        _currentFlyingSpeed += _takeOffSpeed * Time.deltaTime;
                    }

                    if (_destinationDistance > 5f)
                    {
                        _flyingObject.position = Vector3.MoveTowards(_flyingObject.position, _destinationPoint, _currentFlyingSpeed * Time.deltaTime);
                    }
                    else
                    {
                        if (!_reverseDirection && _currentWaypointIndex == (waypoints.Length - 2))
                        {
                            if (!_landOnDestination)
                            {
                                State = DropshipState.Hovering;
                                ArrivedOnDestination();
                                return;
                            }
                            else
                                State = DropshipState.Landing;
                        }

                        if (!_reverseDirection)
                            _currentWaypointIndex++;
                        else
                            _currentWaypointIndex--;

                        CalculateTraveledDistance();
                    }
                }
                else
                {
                    _flyingObject.position = Vector3.MoveTowards(_flyingObject.position, _destinationPoint, _currentFlyingSpeed * Time.deltaTime);
                }

                UpdateRotation();

                if (_slowDown)
                {
                    if (_currentFlyingSpeed > minSpeed)
                    {
                        _currentFlyingSpeed -= _slowDownSpeed * Time.deltaTime;
                    }
                    if (_destinationDistance < 0.1f)
                    {
                        if (_reverseDirection && _currentWaypointIndex == 0)
                        {
                            _currentWaypointIndex--;
                            State = DropshipState.Landing;
                        }
                        else
                            ArrivedOnDestination();
                    }
                }
            }
        }
        #endregion

        #region Public API
        public void StartFlying(bool reloaded = false)
        {
            if (_isDead)
                return;

            if (reloaded)
            {
                ResetShip();
                flewReloaded?.Invoke();
            }

            SetRotateSpeed();
            //Debug.LogError("+++ StartFlying " + _currentWaypointIndex);
            _flying = true;
            State = DropshipState.TakeOff;
        }

        public void Circling()
        {
            _animator?.Play("Circling");
        }
        
        #endregion
        
        #region Private methods
        private void StartFlyingReverse()
        {
            //Debug.LogError("+++ StartFlyingReverse " + _currentWaypointIndex);
            State = _landOnDestination ? DropshipState.TakeOff : DropshipState.Fly;
            SetRotateSpeed();
            _reverseDirection = true;
            _flying = true;
        }

        private void UpdateRotation()
        {
            if (_currentWaypointIndex >= 0 && _currentWaypointIndex < waypoints.Length)
            {
                if (!_reverseDirection)
                    _tmpRot = waypoints[_currentWaypointIndex].rotation;
                else
                    _tmpRot = Quaternion.LookRotation(-waypoints[_currentWaypointIndex].forward, waypoints[_currentWaypointIndex].up);

                _flyingObject.rotation = Quaternion.Slerp(_flyingObject.rotation, _tmpRot, _currentRotateSpeed * Time.deltaTime);
            }
        }

        private void ArrivedOnDestination()
        {
            CalculateTraveledDistance();

            _flying = false;
            _currentFlyingSpeed = 0f;
            //Debug.LogError("+++ ArrivedOnDestination ++++++++++++");        
            if (_reverseDirection)
            {
                returnedOnStartPostion?.Invoke();

                if (_patrolMode)
                    ReloadAndFly();
            }
            else
            {
                _doorAnimator?.SetTrigger("Open");
                arrivedOnDestination?.Invoke();                
            }
            _slowDown = false;
            _partTraveledDistance = 0f;
        }

        private void CargoUnloaded()
        {
            if (!_flying && !_isDead)
            {                
                cargoUnloaded?.Invoke();
                GoOnStartPosition();
            }
        }

        private bool TowardLastWaypoint()
        {
            return (!_reverseDirection && _currentWaypointIndex == (waypoints.Length - 1)) || // forward
                   (_reverseDirection && _currentWaypointIndex <= 0);   // reverse
        }

        private void SetDestination(Vector3 newDestinationPoint)
        {
            if (_destinationPoint.x != newDestinationPoint.x || _destinationPoint.y != newDestinationPoint.y || _destinationPoint.z != newDestinationPoint.z)
            {
                if (_flying)
                    _previousDestinationPoint = new Vector3(_destinationPoint.x, _destinationPoint.y, _destinationPoint.z);
                else
                    _previousDestinationPoint = new Vector3(_flyingObject.position.x, _flyingObject.position.y, _flyingObject.position.z);

                _destinationPoint = newDestinationPoint;
            }

            _destinationDirection = _destinationPoint - _flyingObject.position;
            _destinationDistance = Vector3.Distance(_flyingObject.position, _destinationPoint);
            _traveledDistance = _partTraveledDistance + Vector3.Distance(_flyingObject.position, _previousDestinationPoint);

            if (!_slowDown && _flying && _traveledDistance > _percentPathUntilSlowdown * _totalDistance)
                _slowDown = true;
        }

        private void CalculateTraveledDistance()
        {
            _partTraveledDistance += Vector3.Distance(_previousDestinationPoint, _destinationPoint);
        }

        private int _returnAfterReloadIndex = -1;
        private void ReloadAndFly()
        {
            if (_returnAfterReloadIndex >= 0)
                DelayManager.StopAt(_returnAfterReloadIndex);

            _returnAfterReloadIndex = DelayManager.DelayAction(delegate { StartFlying(true); }, _reloadTime);            
        }
        
        int _setFullRotateSpeed = -1;
        private void SetRotateSpeed()
        {
            var delay = _landOnDestination ? 1f : 0.3f;
            _currentRotateSpeed = _rotateSpeed / 4f;            
            if (_setFullRotateSpeed >= 0)
                DelayManager.StopAt(_setFullRotateSpeed);

            _setFullRotateSpeed = DelayManager.DelayAction(delegate { _currentRotateSpeed = _rotateSpeed; }, delay);            
        }
        
        private int _spawnDelayBlockIndex = -1;
        private void Unship()
        {
            if (_spawnDelayBlockIndex >= 0)
                DelayManager.StopAt(_spawnDelayBlockIndex);

            _spawnDelayBlockIndex = DelayManager.DelayAction(CargoUnloaded, _uloadDelayTime);
            
        }

        private int _returnDelayBlockIndex = -1;
        private void GoOnStartPosition()
        {
            _doorAnimator?.SetTrigger("Close");
            if (_returnDelayBlockIndex >= 0)
                DelayManager.StopAt(_returnDelayBlockIndex);

            _returnDelayBlockIndex = DelayManager.DelayAction(StartFlyingReverse, _waitingTimeAfterUnload);            
        }

        private void CollectAndAssignDamagableParts()
        {
            //if (_destructible == null)
            //    return;

            //var all = _destructible.transform.GetComponentsInChildren<DamagablePart>(true);
            //for (int i = 0; i < all.Length; i++)
            //    all[i].rootDamagable = _destructible;
        }

        private bool IsInAnyHoveringState()
        {
            return State == DropshipState.Hovering || State == DropshipState.Hit;
        }
        #endregion

        #region Listeners
        private void OnDeath()
        {
            _animator.SetInteger("Randomizer", Random.Range(0, 2));
            if (IsInAnyHoveringState())
                State = DropshipState.Fall;
            else
                State = DropshipState.DestructionLanded;

            destroyed?.Invoke();
            //_destructible.SelfRigidbody.mass = 2000f;
            //_destructible.SelfRigidbody.angularDrag = 2000f;
            //_destructible.SelfRigidbody.useGravity = true;
            //_destructible.SelfRigidbody.isKinematic = false;
            _isDead = true;
            //_spawner?.BlockSpawning();
            _deathEffect.SetActive(true);
        }

        private void OnDamaged()
        {
            if (IsInAnyHoveringState() && !_isDead)
            {
                State = DropshipState.Hit;
            }
        }

        #endregion

        #region AnimationEventForwarder

        public void OnDoorOpened()
        {            
            doorOpened?.Invoke();
            Unship();
        }

        public void OnDoorClosed()
        {            
            doorClosed?.Invoke();
        }

        public void OnDoorStartOpening()
        {            
            doorStartOpening?.Invoke();
        }

        public void OnDoorStartClosing()
        {
            doorStartClosing?.Invoke();
        }

        public void OnLandingStarted()
        {            
            landingStart?.Invoke();
        }

        public void OnLandingFinished()
        {            
            landingFinished?.Invoke();
        }

        public void OnTakeOffStarted()
        {            
            takeOffStart?.Invoke();
        }

        public void OnTakeOffFinished()
        {            
            takeOffFinished?.Invoke();
        }

        #endregion

        #region Debug
        private void ResetShip(bool resetRotation = false)
        {
            _reverseDirection = false;
            _flyingObject.position = _startPos;
            if (resetRotation)
                _flyingObject.rotation = _startRot;

            SetDestination(waypoints[0].position);
            _currentWaypointIndex = 0;
            _currentFlyingSpeed = 0f;
            _currentRotateSpeed = 0f;
            //_destructible.SelfRigidbody.mass = 0f;
            //_destructible.SelfRigidbody.angularDrag = 0f;
            //_destructible.SelfRigidbody.useGravity = false;
            //_destructible.SelfRigidbody.isKinematic = true;
            _deathEffect.SetActive(false);
            _slowDown = false;
            _isDead = false;
            State = DropshipState.Landed;
        }

        #if UNITY_EDITOR
        [ContextMenu("Collect And Rename Waypoints")]
        public void CollectAndRenameWaypoints()
        {
            waypoints = new Transform[_waypointsParent.childCount];
            for (int i = 0; i < _waypointsParent.childCount; i++)
            {
                var transformWaypoint = _waypointsParent.GetChild(i);
                var name = "waypoint_" + i;
                if (i == _waypointsParent.childCount - 2)
                    name += "_hoveringPoint";

                if (i == _waypointsParent.childCount - 1)
                {
                    if (!_landOnDestination)
                        transformWaypoint.position = _waypointsParent.GetChild(_waypointsParent.childCount - 2).position;

                    name += "_landingPoint";
                }

                transformWaypoint.gameObject.name = name;
                waypoints[i] = transformWaypoint;

                for (int j = 0; j < transformWaypoint.childCount; j++)
                {
                    var visual = transformWaypoint.GetChild(j);
                    var meshrenderer = visual.GetComponent<MeshRenderer>();
                    if (meshrenderer != null)
                    {
                        visual.gameObject.name = transformWaypoint.gameObject.name + "_visual";
                    }
                }
            }
        }

        //[ContextMenu("Change Waypoints Visual")]
        public void ChangeWaypointsVisual()
        {
            var last = _waypointsParent.GetChild(_waypointsParent.childCount - 1);
            if (last != null)
            {
                for (int i = 0; i < waypoints.Length; i++)
                {
                    var newWaypoint = Instantiate(last.gameObject, waypoints[i].position, waypoints[i].rotation, _waypointsParent);
                    newWaypoint.name = waypoints[i].gameObject.name + "_";
                }
            }
        }

        #endif
        [ContextMenu("Toggle Waypoints Visuals")]
        private void ToggleWaypointsVisuals()
        {
            if (waypoints.Length == 0)
                CollectAndRenameWaypoints();

            for (int i = 0; i < waypoints.Length; i++)
            {
                for (int j = 0; j < waypoints[i].childCount; j++)
                {
                    var child = waypoints[i].GetChild(j);
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                }
            }
        }
        #endregion
    }
}
