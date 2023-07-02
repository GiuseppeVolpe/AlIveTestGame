using UnityEngine;

public class FireSource : MonoBehaviour
{
    public bool OnFromStart;
    public Vector3 CenterOffset;
    public float Radius;
    public GameObject FireParticle;

    private bool _on;
    public bool On
    {
        get
        {
            return _on;
        }

        private set
        {
            _on = value;

            if (_on)
            {
                TurnOn();
            } else
            {
                TurnOff();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        On = OnFromStart;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!On)
        {
            return;
        }

        Collider[] collidersInRange = Physics.OverlapSphere(transform.position + CenterOffset, Radius);

        foreach (Collider collider in collidersInRange)
        {
            FireSource fireSource = collider.GetComponent<FireSource>();

            if (fireSource == this)
            {
                continue;
            }

            if (fireSource != null && fireSource.enabled)
            {
                if (!fireSource.On)
                {
                    fireSource.On = true;
                }
            }
        }
    }

    protected virtual void TurnOn()
    {
        if (FireParticle != null)
        {
            FireParticle.SetActive(true);
        }
    }

    protected virtual void TurnOff()
    {
        if (FireParticle != null)
        {
            FireParticle.SetActive(false);
        }
    }
}
