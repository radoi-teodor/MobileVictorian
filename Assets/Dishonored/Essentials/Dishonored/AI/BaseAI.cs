using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseAI : MonoBehaviour {

    [System.Serializable]
    public class MyEvent: UnityEvent
    {

    }

    /// <summary>
    /// Public variables
    /// </summary>

    public GameObject ragdoll;
    [Space]
    public bool restartOnDie = false;
    [Space]
    public float range = 2.5f;
    [Range(10, 90)]
    public float angle = 30;
    public float timeToForget = 5;
    [Space]
    public bool evil = true;
    public bool isStatic = false;
    [Space]
    public float health = 100;
    public float triggerTimerStart = 15;
    [Space]
    [Space]
    public Animator worldCanvas;

    [Space]
    [Space]
    [Header("Paths")]
    public PathCreator normalPath;
    public PathCreator alertPath;

    [Space]
    [Space]
    [Header("Audios")]
    public AudioClip hurtAud;

    [Space]
    [Space]
    public bool chatUsingChaos = true;
    public List<string> lowChaosSentences = new List<string>();
    public List<string> highChaosSentences = new List<string>();

    [Space]
    public MyEvent chatEvents;
    public MyEvent onExitChatEvents;
    [HideInInspector]
    public bool invoked = false, invokedEnd = false;

    /// <summary>
    /// Private variables
    /// </summary>

    GameObject target;
    Type targetType;

    Animator anim;
    Rigidbody rb;

    Vector3 lastEnemyPos;
    Vector3 startVelocity;

    float timer, triggerTimer = 3f;
    [HideInInspector]
    public float blockDelay = 0, hurtDelay = 0;

    bool dead = false;
    bool triggered = false;
    bool velocityTook = false, velocityUsed = false;

    bool wasKinematic = false, usedGravity = true;

    int pathIndex = 0;
    Vector3[] helperPoints = null;
    float t = 0;

    float deltaTime;

    SkinnedMeshRenderer[] skinnedMesh;
    MeshRenderer[] mesh;

    AudioSourceEssentials audioEssentials;

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {

        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public float AngleFromVector3(Vector3 entity)
    {
        Vector3 direction = entity - transform.position;
        direction.Normalize();
        return Vector3.Angle(transform.forward, direction);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (target != null && GameManager.instance.TimeScale > 0)
        {
            anim.SetLookAtPosition(target.transform.position + transform.up * .8f);
            anim.SetLookAtWeight(.5f);
        }
    }

    // Use this for initialization
    void Start () {

        MakeNormal();

        skinnedMesh = GetComponentsInChildren<SkinnedMeshRenderer>();
        mesh = GetComponentsInChildren<MeshRenderer>();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioEssentials = gameObject.AddComponent<AudioSourceEssentials>();

        gameObject.layer = 10;
        gameObject.tag = "AI";

        if (!isStatic)
        {
            normalPath.path.IsClosed = true;
            alertPath.path.IsClosed = true;
        }

        triggerTimer = triggerTimerStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb)
        {
            if (GameManager.instance.TimeScale <= .05f)
            {

                if (!velocityTook)
                {
                    startVelocity = rb.velocity;

                    wasKinematic = rb.isKinematic;
                    usedGravity = rb.useGravity;

                    rb.isKinematic = true;
                    rb.useGravity = false;

                    velocityTook = true;
                    velocityUsed = false;
                }

            }
            else if (GameManager.instance.TimeScale >= 1f)
            {

                if (!velocityUsed)
                {
                    rb.isKinematic = wasKinematic;
                    rb.useGravity = usedGravity;

                    velocityUsed = true;
                    velocityTook = false;

                    rb.velocity = startVelocity;
                }
            }
        }


        anim.speed = GameManager.instance.TimeScale;
        worldCanvas.speed = GameManager.instance.TimeScale;

        deltaTime = Time.deltaTime * GameManager.instance.TimeScale;

        if (health <= 0 && !dead)
        {
            dead = true;
            worldCanvas.transform.parent = null;
            worldCanvas.SetBool("Triggered", true);
            worldCanvas.SetBool("Alarmed", true);
            Destroy(worldCanvas.gameObject, 5f);

            GameManager.instance.dead++;

            GameObject rd = Instantiate(ragdoll, transform.position, transform.rotation);
            //rd.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().AddForce(-transform.forward * 25, ForceMode.VelocityChange);
            Destroy(gameObject);

            if (restartOnDie)
            {
                GameManager.instance.RestartLevel();
            }
        }
        if (!dead)
        {
            if (evil)
            {
                worldCanvas.SetBool("Triggered", triggered);
                worldCanvas.SetBool("Alarmed", target != null);
            }

            if (hurtDelay >= 0)
            {
                hurtDelay -= deltaTime;
            }

            if (blockDelay > 0)
            {
                blockDelay -= deltaTime;


                anim.SetFloat("Movement", 0);
                anim.SetBool("Running", false);
                anim.SetBool("Attacking", false);

                //agent.ResetPath();

            }
            else
            {
                if (target != null)
                {
                    if (timer <= 0)
                    {
                        target = null;
                    }
                    else
                    {
                        if ((lastEnemyPos - target.transform.position).magnitude > range)
                        {
                            target = null;
                            lastEnemyPos = new Vector3();
                            return;
                        }

                        if (evil)
                        {
                            if ((transform.position - target.transform.position).magnitude > 1.5f)
                            {
                                if (anim.GetFloat("Movement") != 0)
                                {
                                    transform.position = Vector3.MoveTowards(transform.position, target.transform.position - transform.forward, deltaTime * 5);
                                }

                                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position, transform.up), deltaTime * 5f);

                                anim.SetBool("Running", true);
                                anim.SetFloat("Movement", 1);

                            }

                            if ((transform.position - target.transform.position).magnitude > range)
                            {
                                timer -= deltaTime;
                            }

                            if ((transform.position - target.transform.position).magnitude < 2f && Mathf.Abs(transform.position.y - target.transform.position.y) < 1f && hurtDelay <= 0)
                            {
                                anim.SetBool("Running", false);
                                anim.SetFloat("Movement", 0);
                                anim.SetBool("Attacking", true);
                            }
                            else
                            {
                                anim.SetBool("Attacking", false);
                            }

                        }

                        lastEnemyPos = target.transform.position;

                    }
                }
                else
                {
                    timer = timeToForget;
                    anim.SetFloat("Movement", 0);
                    anim.SetBool("Running", false);
                    anim.SetBool("Attacking", false);

                }

                if (!target)
                {

                    Collider[] cols = Physics.OverlapSphere(transform.position, range);

                    foreach (Collider col in cols)
                    {

                        if (Vector3.Angle(transform.forward, (col.transform.position - transform.position).normalized) < angle && Mathf.Abs(transform.position.y - col.transform.position.y) < 2f)
                        {
                            if (VerifyTag(col.gameObject.tag) && col.gameObject.layer == 10)
                            {

                                RaycastHit hit;
                                Debug.DrawLine(transform.position + transform.up + transform.forward, (col.gameObject.transform.position - (transform.position + transform.up + transform.forward)).normalized * 50);
                                if (Physics.Raycast(transform.position + transform.up + transform.forward, (col.gameObject.transform.position - (transform.position + transform.up + transform.forward)).normalized, out hit, Mathf.Infinity))
                                {

                                    if (VerifyTag(hit.collider.tag))
                                    {
                                        StopAllCoroutines();
                                        target = col.gameObject;
                                        targetType = typeof(MotionCore);
                                        lastEnemyPos = target.transform.position;
                                        triggered = true;
                                        break;
                                    }
                                    else
                                    {
                                        anim.SetFloat("Movement", 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (triggered && !target)
            {
                if (triggerTimer >= 0)
                {
                    triggerTimer -= deltaTime;
                }
                else
                {
                    triggered = false;
                    triggerTimer = triggerTimerStart;
                }
            }

            if (!isStatic)
            {
                if (!triggered)
                {
                    if (helperPoints == null)
                    {
                        helperPoints = normalPath.path.GetPointsInSegment(pathIndex);
                    }

                    Vector3 nextClosePoint = Bezier.EvaluateCubic(helperPoints[0], helperPoints[1], helperPoints[2], helperPoints[3], t);
                    if ((transform.position - nextClosePoint).magnitude <= .3f)
                    {
                        t += deltaTime * 3;
                        nextClosePoint = Bezier.EvaluateCubic(helperPoints[0], helperPoints[1], helperPoints[2], helperPoints[3], t);
                    }


                    if ((transform.position - helperPoints[3]).magnitude <= .3f)
                    {
                        if (pathIndex >= normalPath.path.NumSegments - 1)
                        {
                            pathIndex = 0;
                        }
                        else
                        {
                            pathIndex++;
                        }

                        helperPoints = normalPath.path.GetPointsInSegment(pathIndex);
                        t = 0;
                    }

                    anim.SetFloat("Movement", 1);
                    anim.SetBool("Running", false);

                    transform.position = Vector3.MoveTowards(transform.position, nextClosePoint, deltaTime * 3.5f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(nextClosePoint - transform.position, transform.up), deltaTime * 5f);

                }
                else
                {
                    if (!target)
                    {
                        if (helperPoints == null)
                        {
                            helperPoints = alertPath.path.GetPointsInSegment(pathIndex);
                        }

                        Vector3 nextClosePoint = Bezier.EvaluateCubic(helperPoints[0], helperPoints[1], helperPoints[2], helperPoints[3], t);
                        if ((transform.position - nextClosePoint).magnitude <= .3f)
                        {
                            t += deltaTime * 5;
                            nextClosePoint = Bezier.EvaluateCubic(helperPoints[0], helperPoints[1], helperPoints[2], helperPoints[3], t);
                        }


                        if ((transform.position - helperPoints[3]).magnitude <= .3f)
                        {
                            if (pathIndex >= alertPath.path.NumSegments - 1)
                            {
                                pathIndex = 0;
                            }
                            else
                            {
                                pathIndex++;
                            }

                            helperPoints = alertPath.path.GetPointsInSegment(pathIndex);
                            t = 0;
                        }

                        anim.SetFloat("Movement", 1);
                        anim.SetBool("Running", true);

                        transform.position = Vector3.MoveTowards(transform.position, nextClosePoint, deltaTime * 5f);
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(nextClosePoint - transform.position, transform.up), deltaTime * 5f);
                    }
                }
            }
        }


        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        transform.rotation = rot;

    }

    IEnumerator turnAround()
    {

        Quaternion temp = new Quaternion();
        temp.eulerAngles = transform.rotation.eulerAngles;
        temp.eulerAngles = new Vector3(temp.eulerAngles.x, temp.eulerAngles.y - 180, temp.eulerAngles.z);

        while (Mathf.Abs(temp.eulerAngles.y - transform.eulerAngles.y) > 0.1f)
        {

            transform.rotation = Quaternion.Lerp(transform.rotation, temp, deltaTime * 2);
            anim.SetFloat("Movement", 1f);

            yield return null;
        }
        anim.SetFloat("Movement", 0f);
    }

    bool VerifyTag(string goTag)
    {
        if(goTag == "Player")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward * 1.5f, 1);

    }

    public void Attack(float dmg)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position + transform.up + transform.forward * 1.5f, 1);

        bool found = false;

        foreach(Collider col in cols)
        {
            if(col.gameObject == target)
            {
                found = true;
                break;
            }
        }

        if (found)
        {
            float min = dmg - 5 > 0 ? dmg - 5 : dmg;
            float max = dmg + 5;

            if(targetType == typeof(MotionCore))
            {
                MotionCore mc = target.GetComponent<MotionCore>();
                mc.DoDamage(UnityEngine.Random.Range(min, max), this);
            }
        }
    }

    public void DoDamage(float dmg)
    {
        audioEssentials.PlayAudio(hurtAud);

        triggered = true;
        hurtDelay = 1f;

        health -= dmg;
        if (blockDelay <= 0)
        {
            anim.SetBool("Hurt", true);
        }

        if (!evil)
            evil = true;
    }

    public void Blocked()
    {
        blockDelay = 2;
        anim.SetBool("Blocked", true);
        rb.AddForce(-transform.forward * .6f, ForceMode.Impulse);
    }

    public void ResetBlockBool()
    {
        anim.SetBool("Blocked", false);
        rb.velocity = Vector3.zero;
    }

    public void ResetHurtBool()
    {
        anim.SetBool("Hurt", false);
    }

    public GameObject Assasinate()
    {
        dead = true;
        worldCanvas.transform.parent = null;
        worldCanvas.SetBool("Triggered", true);
        worldCanvas.SetBool("Alarmed", true);
        Destroy(worldCanvas.gameObject, 5f);

        GameManager.instance.dead++;

        GameObject rd = Instantiate(ragdoll, transform.position, transform.rotation);
        Destroy(gameObject);

        if (restartOnDie)
        {
            GameManager.instance.RestartLevel();
        }

        return rd;
    }

    public void MakeXRAY()
    {
        StartCoroutine(MakeXRayCor());
    }

    public void MakeNormal()
    {
        StartCoroutine(MakeNormalCor());
    }

    IEnumerator MakeXRayCor()
    {
        if(mesh.Length > 0)
        {
            while(mesh[0].sharedMaterials[0].GetColor("_OutlineColor").a < .95)
            {
                foreach(MeshRenderer m in mesh)
                {
                    foreach (Material mat in m.sharedMaterials)
                    {
                        if(mat.HasProperty("_OutlineColor")) {
                            Color c = mat.GetColor("_OutlineColor");
                            mat.SetColor("_OutlineColor", Color.Lerp(c, new Color(c.r, c.g, c.b, 1), Time.deltaTime * 3));

                        }

                    }
                }
                yield return null;
            }

            foreach (MeshRenderer m in mesh)
            {
                foreach (Material mat in m.sharedMaterials)
                {
                    if (mat.HasProperty("_OutlineColor"))
                    {
                        Color c = mat.GetColor("_OutlineColor");
                        mat.SetColor("_OutlineColor", new Color(c.r, c.g, c.b, 1));
                    }
                }
            }
        }
        else if (skinnedMesh.Length > 0)
        {
            while (skinnedMesh[0].sharedMaterials[0].GetColor("_OutlineColor").a < .95)
            {
                foreach (SkinnedMeshRenderer m in skinnedMesh)
                {
                    foreach (Material mat in m.sharedMaterials)
                    {
                        if(mat.HasProperty("_OutlineColor")) {
                            Color c = mat.GetColor("_OutlineColor");
                            mat.SetColor("_OutlineColor", Color.Lerp(c, new Color(c.r, c.g, c.b, 1), Time.deltaTime * 3));
                        }
                    }
                }
                yield return null;
            }

            foreach (SkinnedMeshRenderer m in skinnedMesh)
            {
                foreach (Material mat in m.sharedMaterials)
                {
                    if (mat.HasProperty("_OutlineColor"))
                    {
                        Color c = mat.GetColor("_OutlineColor");
                        mat.SetColor("_OutlineColor", new Color(c.r, c.g, c.b, 1));
                    }
                }
            }
        }
    }

    IEnumerator MakeNormalCor()
    {
        if (mesh != null && mesh.Length > 0)
        {
            while (mesh[0].sharedMaterials[0].GetColor("_OutlineColor").a > 0f)
            {
                foreach (MeshRenderer m in mesh)
                {
                    foreach (Material mat in m.sharedMaterials)
                    {
                        if(mat.HasProperty("_OutlineColor"))
                        {
                            Color c = mat.GetColor("_OutlineColor");
                            mat.SetColor("_OutlineColor", Color.Lerp(c, new Color(c.r, c.g, c.b, -0.1f), Time.deltaTime * 3));
                        }
                    }
                }
                yield return null;
            }

            foreach (MeshRenderer m in mesh)
            {
                foreach (Material mat in m.sharedMaterials)
                {
                    if (mat.HasProperty("_OutlineColor"))
                    {
                        Color c = mat.GetColor("_OutlineColor");
                        mat.SetColor("_OutlineColor", new Color(c.r, c.g, c.b, 0f));
                    }
                }
            }
        }
        else if (skinnedMesh != null && skinnedMesh.Length > 0)
        {
            while (skinnedMesh[0].sharedMaterials[0].GetColor("_OutlineColor").a > 0f)
            {
                foreach (SkinnedMeshRenderer m in skinnedMesh)
                {
                    foreach (Material mat in m.sharedMaterials)
                    {
                        if(mat.HasProperty("_OutlineColor"))
                        {
                            Color c = mat.GetColor("_OutlineColor");
                            mat.SetColor("_OutlineColor", Color.Lerp(c, new Color(c.r, c.g, c.b, -0.1f), Time.deltaTime * 3));
                        }
                    }
                }
                yield return null;
            }

            foreach (SkinnedMeshRenderer m in skinnedMesh)
            {
                foreach (Material mat in m.sharedMaterials)
                {
                    if (mat.HasProperty("_OutlineColor"))
                    {
                        Color c = mat.GetColor("_OutlineColor");
                        mat.SetColor("_OutlineColor", new Color(c.r, c.g, c.b, 0f));
                    }
                }
            }
        }
    }

    public void PublicSetTalkLowChaos(string subs)
    {

        List<string> actualSubs = new List<string>();
        string[] subsArray = subs.Split('*');

        if (subsArray.Length > 0)
        {
            foreach (string s in subsArray)
            {
                actualSubs.Add(s);
            }
        }
        else
        {
            actualSubs.Add(subs);
        }

        lowChaosSentences = actualSubs;
    }

    public void PublicSetTalkHighChaos(string subs)
    {

        List<string> actualSubs = new List<string>();
        string[] subsArray = subs.Split('*');

        if (subsArray.Length > 0)
        {
            foreach (string s in subsArray)
            {
                actualSubs.Add(s);
            }
        }
        else
        {
            actualSubs.Add(subs);
        }

        highChaosSentences = actualSubs;
    }

    public void SetExitEvents(GameObject eventsGO)
    {
        invokedEnd = false;
        StartCoroutine(setExitEvents(eventsGO.GetComponent<EventHolder>().events));
    }

    IEnumerator setExitEvents(MyEvent events)
    {
        yield return null;
        onExitChatEvents = events;
    }

    public void SetStartEvents(GameObject eventsGO)
    {
        invoked = false;
        StartCoroutine(setStartEvents(eventsGO.GetComponent<EventHolder>().events));
    }

    IEnumerator setStartEvents(MyEvent events)
    {
        yield return null;
        chatEvents = events;
    }
}
