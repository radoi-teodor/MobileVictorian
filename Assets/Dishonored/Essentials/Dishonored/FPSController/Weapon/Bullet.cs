using UnityEngine;

public class Bullet : MonoBehaviour {

    public Vector3 direction;
    public float speed;

    [HideInInspector]
    public float damage;
	
	// Update is called once per frame
	void Update () {
        transform.Translate(direction.normalized * speed * GameManager.instance.TimeScale * Time.deltaTime, Space.World);

        if(GameManager.instance.TimeScale > .1f)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, direction.normalized, out hit, Mathf.Infinity))
            {
                if(hit.collider.gameObject.tag == "AI")
                {
                    BaseAI bai = hit.collider.gameObject.GetComponent<BaseAI>();
                    bai.Blocked();
                    bai.DoDamage(damage);

                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "AI")
        {
            BaseAI bai = collision.collider.gameObject.GetComponent<BaseAI>();
            bai.Blocked();
            bai.DoDamage(damage);            

            Destroy(gameObject);
        }else if(collision.gameObject.tag == "Player" && GameManager.instance.TimeScale <= .1f)
        {
            MotionCore mc = collision.collider.gameObject.GetComponent<MotionCore>();
            mc.DoDamage(damage);

            Destroy(gameObject);
        }
    }
}
