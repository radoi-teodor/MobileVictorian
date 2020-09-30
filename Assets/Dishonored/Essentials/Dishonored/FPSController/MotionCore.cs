using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MotionCore : MonoBehaviour {

    /// <summary>
    /// Public variables
    /// </summary>

    [Header("General")]
    public bool hasSword = true;
    public bool hasGun = true;
    public bool hasCrossbow = true;
    [Space]
    public List<SkillBase> skills = new List<SkillBase>();

    
    [Space]
    [Space]
    [Header("Hands")]
    public Animator handsCharAnim;
    public Animator mainCamAnim;
    [Space]
    public GameObject rightHandPoint;
    public GameObject leftHandPoint;

    [Space]
    public GameObject sword;
    public GameObject gun;
    public GameObject crossbow;

    [Space]
    [Space]
    [Header("Acrobatics")]
    public GameObject acrobaticsPivot;
    public Camera acrobaticsCamera;


    [Space]
    [Space]
    [Header("Components")]
    public CapsuleCollider normalCapsule;
    public CapsuleCollider crouchCapsule;

    [Space]
    [Space]
    [Header("Audios")]
    public AudioClip swordTakeoutAud;
    public AudioClip gunTakeoutAud;
    public AudioClip gunShotAud;
    public AudioClip crossbowShotAud;
    public AudioClip fallAud;
    [Space]
    public AudioClip hurtAud;
    public AudioClip swordBlockAud;

    [Space]
    [Space]
    [Header("UI")]
    public CanvasScaler cs;
    [Space]
    public ProgressBarEssential healthProgress;
    public ProgressBarEssential manaProgress;
    public ProgressBarEssential manaFutureProgress;
    [Space]
    public Image bloodScreenImg;
    [Space]
    public Animator menuAnim;
    public GameObject selectorCursor;
    public RectTransform skillCircle;
    public Text skillTX;
    public GameObject controlsRig;
    [Space]
    public GameObject skillHolder;
    public RectTransform skillPrefab;
    public RectTransform baseSkillPrefab;
    [Space]
    public Sprite gunIcon;
    public Sprite crossbowIcon;
    [Space]
    public GameObject skillPan;
    public Image skillImg;
    public GameObject bulletPan;
    public Text bulletTX;
    [Space]
    public Image secondCursor;
    public Sprite skillCursor;
    [Space]
    public RectTransform missionPointerPan;
    public Text missionTX;
    [Space]
    public RectTransform chatPanTransform;
    public RectTransform usePanTransform;

    [Space]
    [Header("Mobile UI")]
    public GameObject useButton;
    public List<GameObject> hideWhenAbilityMenuShown = new List<GameObject>();

    /// <summary>
    /// Private variables
    /// </summary>

    bool rightEquiped = false, leftEquiped = false, crouched = false;
    bool hurt = false;
    bool jumped = false, climbing = false;
    bool menuShown = false;

    bool velocityTook = false, velocityUsed = false;

    float holdF = 0, airTime = 0, blockTimer = 0;
    [HideInInspector]
    public float health = 100, mana = 100;
    float manaFuture = 100;

    float skillCooldown = 0;
    float deltaTime;

    [HideInInspector]
    public string skillEquipped = "";

    Vector3 startVelocity;

    SkillBase activeSkill;

    Animator anim;
    Rigidbody rb;

    [HideInInspector]
    public AudioSourceEssentials audioGeneral;
    [HideInInspector]
    public RigidbodyFirstPersonController controller;

    Volume postProcess;
    GameObject bobCam;
    WeaponCore gunWeapon, swordWeapon, crossbowWeapon;

    Vignette vignette;
    ColorAdjustments colorGrading;

    [HideInInspector]
    public Camera bobCamComp;

    BaseAI assasinateEnemy = null;
    GameObject assasinateBody = null;

    GameObject lastSelectedObject = null;
    [HideInInspector]
    public MissionPoint missionPoint = null;

    bool hasGunMemory, hasSwordMemory, hasCrossbowMemory;
    int skillCount;

    bool controllerWasEnabled = true, wasKinematic = false, usedGravity = true;

    float timeScale = 1, masterTimeScale = 1;

    bool dead = false;
    bool hasBeenInAir = true;

    // Use this for initialization
    void Start () {

        cs.referenceResolution = new Vector2(Screen.width, Screen.height);

        audioGeneral = gameObject.AddComponent<AudioSourceEssentials>();
        controller = GetComponent<RigidbodyFirstPersonController>();
        bobCam = controller.cam.gameObject;
        bobCamComp = controller.cam;
        postProcess = controller.cam.GetComponent<Volume>();

        postProcess.profile.TryGet<Vignette>(out vignette);
        postProcess.profile.TryGet<ColorAdjustments>(out colorGrading);

        gunWeapon = gun.GetComponent<WeaponCore>();
        swordWeapon = sword.GetComponent<WeaponCore>();
        crossbowWeapon = crossbow.GetComponent<WeaponCore>();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        missionPoint = FindObjectOfType<MissionPoint>();

        SetWeapon(sword, swordWeapon.localPosition, swordWeapon.localRotation);
        SetWeapon(gun, gunWeapon.localPosition, gunWeapon.localRotation);
        SetWeapon(crossbow, crossbowWeapon.localPosition, crossbowWeapon.localRotation);

        sword.SetActive(false);
        gun.SetActive(false);
        crossbow.SetActive(false);

        normalCapsule.enabled = true;
        crouchCapsule.enabled = false;

        mainCamAnim.cullingMode = AnimatorCullingMode.CullCompletely;

        gameObject.layer = 10;

        healthProgress.max = health;
        manaProgress.max = mana;

        SetSaturation(0);

        hasGunMemory = hasGun;
        hasSwordMemory = hasSword;
        hasCrossbowMemory = hasCrossbow;
        skillCount = skills.Count;

        RefreshSkills();

    }

    void RefreshSkills()
    {
        foreach (Transform child in skillHolder.transform)
        {
            Destroy(child.gameObject);
        }

        if (skills.Count > 0 || hasCrossbow || hasGun)
        {
            float skillUIUnit = 360 / (skills.Count + (hasGun ? 1 : 0) + (hasCrossbow ? 1 : 0));

            int skillMenuCounter = 0;

            foreach (SkillBase skill in skills)
            {
                skill.owner = this;

                Vector2 imgPos = Bezier.CircleTransform(Vector3.zero, skillMenuCounter * skillUIUnit, 150);
                RectTransform rt = Instantiate(skillPrefab.gameObject, skillHolder.transform).GetComponent<RectTransform>();
                rt.anchoredPosition = imgPos;

                rt.GetChild(0).gameObject.GetComponent<Image>().sprite = skill.skillIcon;

                SkillUI sui = rt.GetComponent<SkillUI>();
                sui.anchoredPos = imgPos;
                sui.powerName = skill.powerName;

                skillMenuCounter++;
            }

            if (hasCrossbow)
            {
                Vector2 imgPos = Bezier.CircleTransform(Vector3.zero, skillMenuCounter * skillUIUnit, 150);
                RectTransform rt = Instantiate(baseSkillPrefab.gameObject, skillHolder.transform).GetComponent<RectTransform>();
                rt.anchoredPosition = imgPos;

                rt.GetChild(0).gameObject.GetComponent<Image>().sprite = crossbowIcon;

                SkillUI sui = rt.GetComponent<SkillUI>();
                sui.anchoredPos = imgPos;
                sui.powerName = "Crossbow";

                skillMenuCounter++;

            }

            if (hasGun)
            {
                Vector2 imgPos = Bezier.CircleTransform(Vector3.zero, skillMenuCounter * skillUIUnit, 150);
                RectTransform rt = Instantiate(baseSkillPrefab.gameObject, skillHolder.transform).GetComponent<RectTransform>();
                rt.anchoredPosition = imgPos;

                rt.GetChild(0).gameObject.GetComponent<Image>().sprite = gunIcon;

                SkillUI sui = rt.GetComponent<SkillUI>();
                sui.anchoredPos = imgPos;
                sui.powerName = "Gun";

                skillEquipped = "Gun";
                activeSkill = null;

                skillMenuCounter++;
            }
            else
            {
                if (skills.Count > 0)
                {
                    skillEquipped = skills[0].powerName;
                    activeSkill = skills[0];
                }
                else
                {
                    if (hasCrossbow)
                    {
                        skillEquipped = "Crossbow";
                        activeSkill = null;
                    }
                    else
                    {
                        skillEquipped = "";
                        activeSkill = null;
                    }
                }
            }
        }
        else
        {
            skillEquipped = "";
            activeSkill = null;
        }
    }

    // Update is called once per frame
    void Update () {

        controlsRig.SetActive(!GameManager.instance.paused);
       
        deltaTime = Time.deltaTime * GameManager.instance.MasterTimeScale;

        handsCharAnim.speed = GameManager.instance.MasterTimeScale;

        if (rb)
        {
            if (GameManager.instance.MasterTimeScale <= .05f)
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
            else if (GameManager.instance.MasterTimeScale >= 1f)
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

        if (GameManager.instance.paused)
        {
            return;
        }

        if(health <= 1 && !dead)
        {
            dead = true;
            GameManager.instance.RestartLevel();
        }

        chatPanTransform.gameObject.SetActive(false);
        usePanTransform.gameObject.SetActive(false);

        if (!missionPoint)
        {
            missionPointerPan.gameObject.SetActive(false);
        }
        else
        {
            missionTX.text = missionPoint.missionText;

            
            Vector3 head = (missionPoint.gameObject.transform.position - transform.position).normalized;
            float forwardDot = Vector3.Dot(head, transform.forward);
            float rightDot = Vector3.Dot(head, transform.right);

            if (forwardDot > .85f)
            {
                Vector2 crossPos = bobCamComp.WorldToViewportPoint(missionPoint.transform.position);
                float x = cs.referenceResolution.x * crossPos.x;
                float y = cs.referenceResolution.y * crossPos.y;

                missionPointerPan.anchoredPosition = Vector2.Lerp(missionPointerPan.anchoredPosition, new Vector2(x, y), Time.deltaTime * 10);
            }
            else
            {
                float x = 0;
                float y = bobCamComp.WorldToViewportPoint(missionPoint.transform.position).y * cs.referenceResolution.y;
                y = Mathf.Clamp(y, missionPointerPan.sizeDelta.y / 2, cs.referenceResolution.y - cs.referenceResolution.y/2);

                if (rightDot > 0)
                {
                    x = cs.referenceResolution.x - missionPointerPan.sizeDelta.x / 2;
                }
                else
                {
                    x = missionPointerPan.sizeDelta.x / 2;
                }

                missionPointerPan.anchoredPosition = Vector2.Lerp(missionPointerPan.anchoredPosition, new Vector2(x, y), Time.deltaTime * 10);
            }
        }

        if(skillEquipped != "Gun" && !menuShown)
        {
            gun.SetActive(false);
        }

        if (skillEquipped != "Crossbow" && !menuShown)
        {
            crossbow.SetActive(false);
        }

        if (hasCrossbowMemory != hasCrossbow || hasGunMemory != hasGun || skillCount != skills.Count || hasSwordMemory != hasSword)
        {
            hasGunMemory = hasGun;
            hasCrossbowMemory = hasCrossbow;
            skillCount = skills.Count;

            hasSwordMemory = hasSword;

            RefreshSkills();

            StartCoroutine(reequip());
        }

        healthProgress.value = health;
        manaProgress.value = mana;
        manaFutureProgress.value = manaFuture;

        bloodScreenImg.color = Color.Lerp(bloodScreenImg.color, new Color(1, 1, 1, 0), Time.unscaledDeltaTime);
        acrobaticsCamera.transform.localPosition = bobCam.transform.localPosition;

        menuShown = ControlFreak2.CF2Input.GetMouseButton(2);


        // MOBILE CONTROLS
        
        if (ControlFreak2.CF2Input.GetMouseButtonDown(2)) {
            foreach (GameObject item in hideWhenAbilityMenuShown)
            {
                item.SetActive(false);
            }
        }
        else if(ControlFreak2.CF2Input.GetMouseButtonUp(2)) {
            foreach (GameObject item in hideWhenAbilityMenuShown)
            {
                item.SetActive(true);
            }
        }
        
        // MOBILE CONTROLS



        if (!menuShown)
        {
            if (skillEquipped == "Gun" || skillEquipped == "Crossbow")
            {
                bulletPan.SetActive(true);
            }
            else
            {
                bulletPan.SetActive(false);
            }

            if (skillEquipped == "Gun")
            {
                bulletTX.text = gunWeapon.bullets.ToString();
                skillImg.sprite = gunIcon;
                secondCursor.sprite = gunWeapon.cursor;
            }
            else if (skillEquipped == "Crossbow")
            {
                bulletTX.text = crossbowWeapon.bullets.ToString();
                skillImg.sprite = crossbowIcon;
                secondCursor.sprite = crossbowWeapon.cursor;
            }
            else
            {
                if (skillEquipped != "" || activeSkill != null)
                {
                    secondCursor.sprite = skillCursor;
                }

                if (activeSkill)
                {
                    skillImg.sprite = activeSkill.skillIcon;
                }
            }

            if (skillEquipped == "" && activeSkill == null)
            {
                if (hasSword)
                {
                    secondCursor.sprite = swordWeapon.cursor;
                }
                else
                {
                    secondCursor.gameObject.SetActive(false);
                }
                skillPan.SetActive(false);
            }
            else
            {
                secondCursor.gameObject.SetActive(true);
                skillPan.SetActive(true);
            }
        }


        menuAnim.SetBool("Shown", menuShown);

        if (ControlFreak2.CF2Input.GetMouseButtonDown(2))
        {
            ControlFreak2.CFCursor.visible = true;
            ControlFreak2.CFCursor.lockState = CursorLockMode.None;

            controllerWasEnabled = controller.enabled;
            controller.enabled = false;

            if (GameManager.instance.deltaTimeScale > 0)
            {
                timeScale = 1;
            }
            else if (GameManager.instance.deltaTimeScale < 0)
            {
                timeScale = GameManager.instance.nextTimeScale;
            }
            else
            {
                timeScale = GameManager.instance.TimeScale;
            }

            GameManager.instance.TimeScale = 0;
            GameManager.instance.MasterTimeScale = 0;

            skillTX.text = skillEquipped!=""?skillEquipped:"None";
        }

        if (ControlFreak2.CF2Input.GetMouseButtonUp(2) || (!GameManager.instance.paused && ControlFreak2.CF2Input.GetKeyDown(GameManager.instance.pauseKey)))
        {
            ControlFreak2.CFCursor.visible = false;
            ControlFreak2.CFCursor.lockState = CursorLockMode.Locked;

            controller.enabled = controllerWasEnabled;

            GameManager.instance.TimeScale = timeScale;
            GameManager.instance.MasterTimeScale = 1;
        }

        if (menuShown)
        {
            Vector3 mousePosition = Input.mousePosition;

            if (
                mousePosition.x > Screen.width / 2 - skillCircle.rect.width/2 &&
                mousePosition.x < Screen.width / 2 + skillCircle.rect.width / 2 &&
                mousePosition.y > Screen.height / 2 - skillCircle.rect.height / 2 &&
                mousePosition.y < Screen.height / 2 + skillCircle.rect.height / 2
                )
            {
                Vector2 dir = mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                selectorCursor.transform.rotation = Quaternion.Lerp(selectorCursor.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.unscaledDeltaTime * 12);
            }
        }

        if (!hurt)
        {
            if (ControlFreak2.CF2Input.GetMouseButtonDown(0) && !rightEquiped && hasSword)
            {
                rightEquiped = true;
                handsCharAnim.SetBool("RightEquiped", true);
            }
            else if (ControlFreak2.CF2Input.GetMouseButtonDown(0) && rightEquiped)
            {
                Collider[] cols = Physics.OverlapSphere(transform.position + transform.up + transform.forward * 2f, 1.5f);

                BaseAI enemy = null;

                foreach (Collider col in cols)
                {
                    if (col.gameObject.tag == "AI")
                    {
                        enemy = col.gameObject.GetComponent<BaseAI>();
                        break;
                    }
                }

                if (enemy)
                {
                    float angleBetween = transform.eulerAngles.y - enemy.transform.eulerAngles.y;
                    if ((angleBetween < 20 || angleBetween > 340) && angleBetween > 0)
                    {
                        assasinateEnemy = enemy;
                        controller.enabled = false;
                        handsCharAnim.SetBool("Assasinate", true);
                    }
                    else
                    {
                        handsCharAnim.SetBool("RightAttack", true);
                    }
                }
                else
                {
                    handsCharAnim.SetBool("RightAttack", true);
                }
            }
        }

        handsCharAnim.SetBool("IsGun", skillEquipped == "Gun" || skillEquipped == "Crossbow");

        if(skillCooldown >= 0 && !ControlFreak2.CF2Input.GetMouseButton(1))
        {
            skillCooldown -= deltaTime;
        }

        if (!hurt)
        {
            if (ControlFreak2.CF2Input.GetMouseButtonDown(1) && !leftEquiped && (hasGun || (skillEquipped != "Gun" && activeSkill)))
            {
                leftEquiped = true;

                handsCharAnim.SetBool("LeftEquiped", true);
            }
            else if (ControlFreak2.CF2Input.GetMouseButtonDown(1) && leftEquiped)
            {
                if ((skillEquipped == "Gun" && gunWeapon.bullets > 0) || (skillCooldown <= 0 && (skillEquipped != "Gun" && skillEquipped != "Crossbow") && activeSkill && mana - activeSkill.manaCost >= 0) || (skillEquipped == "Crossbow" && crossbowWeapon.bullets > 0))
                {
                    handsCharAnim.SetBool("LeftAttack", true);

                    
                    if (activeSkill && activeSkill.isHoldable)
                    {
                        activeSkill.StartSkill();
                    }
                    else if (activeSkill && !activeSkill.isHoldable)
                    {
                        activeSkill.ActivateSkill();

                        if(manaFuture - mana != 20)
                        {
                            manaFuture = mana + 20;
                        }

                        skillCooldown = 1;
                    }
                }
            }
        }

        if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.LeftControl) && rightEquiped)
        {
            blockTimer = 0;
            blockTimer += deltaTime;
            handsCharAnim.SetBool("RightBlock", true);
        }

        if (ControlFreak2.CF2Input.GetAxis("Jump")>0)
        {
            jumped = controller.Grounded;
            hasBeenInAir = false;
        }

        if(!hasBeenInAir && !controller.Grounded)
        {
            hasBeenInAir = true;
        }

        if (controller.Grounded && hasBeenInAir)
        {
            hasBeenInAir = false;
            jumped = false;
        }

        if(blockTimer != 0 && blockTimer < 1)
        {
            blockTimer += deltaTime;
        }else if(blockTimer >= 1)
        {
            blockTimer = 0;
        }

        if ((leftEquiped || rightEquiped) && ControlFreak2.CF2Input.GetKey(KeyCode.F) && !hurt)
        {
            holdF += deltaTime;
        }

        if (holdF > .75f || ControlFreak2.CF2Input.GetKeyDown(KeyCode.X))
        {
            if (rightEquiped)
            {
                handsCharAnim.SetBool("RightEquiped", false);
            }

            if (leftEquiped)
            {
                handsCharAnim.SetBool("LeftEquiped", false);
            }

            rightEquiped = false;
            leftEquiped = false;
            holdF = 0f;
        }

        if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.C) && !hurt)
        {
            crouched = !crouched;
            CrouchSet();
        }


        if(skillEquipped == "Gun" || skillEquipped == "Crossbow")
        {
            handsCharAnim.SetBool("IsHoldable", false);
        }
        else if(activeSkill)
        {
            handsCharAnim.SetBool("IsHoldable", activeSkill.isHoldable);


            if (activeSkill.isHoldable)
            {

                if (skillCooldown <= 0)
                {
                    bool isHolding = ControlFreak2.CF2Input.GetMouseButton(1);

                    handsCharAnim.SetBool("LeftAttack", isHolding);

                    if (isHolding)
                    {
                        activeSkill.LoopSkill();
                    }

                    if (ControlFreak2.CF2Input.GetMouseButtonUp(1))
                    {
                        activeSkill.ActivateSkill();

                        if (manaFuture - mana != 20)
                        {
                            manaFuture = mana + 20;
                        }

                        skillCooldown = 1;
                    }
                }
            }
        }

        handsCharAnim.SetFloat("Move", ControlFreak2.CF2Input.GetAxis("Vertical"));
        handsCharAnim.SetBool("Running", ControlFreak2.CF2Input.GetKey(KeyCode.LeftShift));
        handsCharAnim.SetBool("Crouched", crouched);

        if (crouched)
        {
            if(GetVignette() < .4f)
            {
                SetVignette(Mathf.Lerp(GetVignette(), .5f, deltaTime * 5));
            }

            if (bobCam.transform.localPosition.y > .1f)
            {
                bobCam.transform.localPosition = Vector3.Lerp(bobCam.transform.localPosition, new Vector3(bobCam.transform.localPosition.x, .1f, bobCam.transform.localPosition.z), Time.deltaTime * 3);
            }

            if (ControlFreak2.CF2Input.GetKey(KeyCode.LeftShift))
            {
                crouched = false;
                CrouchSet();
            }
        }
        else
        {
            if (GetVignette() > .05f)
            {
                SetVignette(Mathf.Lerp(GetVignette(), 0, deltaTime * 5));
            }

            if (bobCam.transform.localPosition.y < .6f)
            {
                bobCam.transform.localPosition = Vector3.Lerp(bobCam.transform.localPosition, new Vector3(bobCam.transform.localPosition.x, .6f, bobCam.transform.localPosition.z), Time.unscaledDeltaTime * 3);
            }
        }

        if (!controller.Grounded && !climbing)
        {
            airTime += deltaTime;
        }
        else
        {
            if (airTime > 0)
            {
                if (airTime > 1f)
                {
                    mainCamAnim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    handsCharAnim.SetTrigger("Fall");
                    mainCamAnim.SetTrigger("Fall");
                    audioGeneral.PlayAudio(fallAud);
                    controller.enabled = false;
                }

                airTime = 0;
            }
        }

    }

    private void LateUpdate()
    {

        if (GameManager.instance.paused)
        {
            return;
        }

        bool useButtonActive = false;

        RaycastHit hit;
        if (Physics.Raycast(bobCamComp.ViewportPointToRay(new Vector3(.5f, .5f, 0)), out hit, 3.5f))
        {


            if (hit.collider.tag == "Trigger")
            {
                useButtonActive = true;
                if (lastSelectedObject && hit.collider.gameObject != lastSelectedObject)
                {
                    lastSelectedObject.SendMessage("SetFresnel", 0f);
                }

                usePanTransform.gameObject.SetActive(true);

                Vector2 crossPos = bobCamComp.WorldToViewportPoint(hit.collider.transform.position);
                float x = cs.referenceResolution.x * crossPos.x;
                float y = cs.referenceResolution.y * crossPos.y;

                usePanTransform.anchoredPosition = new Vector2(x, y);

                lastSelectedObject = hit.collider.gameObject;

                hit.collider.SendMessage("SetFresnel", 80f);

                if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.F))
                {
                    hit.collider.SendMessage("TriggerSystem", gameObject);
                }

            }else if(hit.collider.tag == "AI")
            {
                useButtonActive = true;

                chatPanTransform.gameObject.SetActive(true);

                Vector2 crossPos = bobCamComp.WorldToViewportPoint(hit.collider.transform.position);
                float x = cs.referenceResolution.x * crossPos.x;
                float y = cs.referenceResolution.y * crossPos.y;

                chatPanTransform.anchoredPosition = new Vector2(x,y);

                if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.F))
                {
                    BaseAI talkAI = hit.collider.gameObject.GetComponent<BaseAI>();

                    if (!talkAI.invokedEnd)
                    {
                        talkAI.invokedEnd = true;
                        GameManager.instance.onExitChatEvent = talkAI.onExitChatEvents;
                    }

                    if (!talkAI.invoked)
                    {
                        talkAI.invoked = true;
                        talkAI.chatEvents.Invoke();
                    }

                    if (GameManager.instance.lowChaos || !talkAI.chatUsingChaos)
                    {
                        if (talkAI.lowChaosSentences != null && talkAI.lowChaosSentences.Count > 0)
                        {
                            GameManager.instance.SetSubtitle(talkAI.lowChaosSentences);
                        }
                    }
                    else
                    {
                        if (talkAI.highChaosSentences!= null && talkAI.highChaosSentences.Count > 0)
                        {
                            GameManager.instance.SetSubtitle(talkAI.highChaosSentences);
                        }
                    }
                }
            }
        }
        else
        {
            if (lastSelectedObject)
            {
                lastSelectedObject.SendMessage("SetFresnel", 0f, SendMessageOptions.DontRequireReceiver);
            }
        }
        useButton.SetActive(useButtonActive);

        mana = Mathf.Lerp(mana, manaFuture, Time.deltaTime / 5);
    }

    public void CrouchSet()
    {
        if (crouched)
        {
            crouchCapsule.enabled = true;
            normalCapsule.enabled = false;
        }
        else
        {
            crouchCapsule.enabled = false;
            normalCapsule.enabled = true;
        }
    }

    public void SetWeapon(GameObject weapon, Vector3 pos, Vector3 rot)
    {
        weapon.transform.localPosition = pos;

        Quaternion temp_q = new Quaternion();
        temp_q.eulerAngles = rot;
        weapon.transform.localRotation = temp_q;
    }

    void SetVignette(float amount)
    {
        vignette.intensity.value = amount;
    }

    float GetVignette()
    {
        return vignette.intensity.value;
    }

    public void SetSaturation(float amount)
    {
        colorGrading.saturation.value = amount;
    }

    public float GetSaturation()
    {
        return colorGrading.saturation.value;
    }

    public void RightTogFunc() {
        audioGeneral.PlayAudio(swordTakeoutAud);
        sword.SetActive(rightEquiped);
    }

    public void LeftTogFunc()
    {
        audioGeneral.PlayAudio(gunTakeoutAud);

        if (skillEquipped == "Gun" && hasGun)
        {
            gun.SetActive(leftEquiped);
        }else if(skillEquipped == "Crossbow" && hasCrossbow)
        {
            crossbow.SetActive(leftEquiped);
        }
    }

    public void ResetSwordAttackFunc()
    {
        handsCharAnim.SetBool("RightAttack", false);
    }

    public void BlockResetFunc()
    {
        handsCharAnim.SetBool("RightBlock", false);
    }

    public void ShootFunc()
    {
        if (skillEquipped == "Gun")
        {
            audioGeneral.PlayAudio(gunShotAud);

            Ray camRay = bobCamComp.ViewportPointToRay(new Vector3(.5f, .5f, 0));

            Instantiate(gunWeapon.explosion, gunWeapon.shootPoint.transform.position, Quaternion.identity).transform.SetParent(gunWeapon.shootPoint.transform);
            Bullet bul = Instantiate(gunWeapon.bullet.gameObject, camRay.origin + camRay.direction * .75f, Quaternion.identity).GetComponent<Bullet>();


            bul.direction = camRay.direction;

            bul.speed = 35f;
            bul.damage = gunWeapon.damage;

            gunWeapon.bullets--;
        }
        else if (skillEquipped == "Crossbow")
        {
            audioGeneral.PlayAudio(crossbowShotAud);

            Ray camRay = bobCamComp.ViewportPointToRay(new Vector3(.5f, .5f, 0));

            Quaternion bulRot = bobCam.transform.rotation;
            bulRot.eulerAngles += new Vector3(90,0,0);

            Bullet bul = Instantiate(crossbowWeapon.bullet.gameObject, camRay.origin + camRay.direction * 1.5f, bulRot).GetComponent<Bullet>();
            
            bul.direction = camRay.direction;

            bul.speed = 50f;
            bul.damage = crossbowWeapon.damage;

            crossbowWeapon.bullets--;
        }
    }

    public void ResetGunAttackFunc()
    {
        handsCharAnim.SetBool("LeftAttack", false);
    }

    public void StartAssasinateFunc()
    {
        if (!assasinateBody)
        {
            assasinateBody = assasinateEnemy.Assasinate();

            assasinateBody.layer = 9;
            assasinateBody.transform.parent = gun.transform.parent;

            assasinateBody.transform.localPosition = -transform.up + transform.forward * .75f + transform.right * .25f;
        }
    }

    public void DetachBodyFunc()
    {
        if (assasinateBody)
        {
            assasinateBody.transform.parent = null;
            assasinateBody.layer = 0;
        }

        assasinateBody = null;

    }

    public void ResetAssasianteFunc()
    {

        controller.enabled = true;
        assasinateEnemy = null;
        handsCharAnim.SetBool("Assasinate", false);
    }

    public void ResetCameraCullingFunc()
    {
        mainCamAnim.cullingMode = AnimatorCullingMode.CullCompletely;
        controller.enabled = true;
        controller.mouseLook.Init(transform, controller.cam.transform);
    }

    public void AttackFunc(float dmg)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position + transform.up + transform.forward * 2f, 1.5f);

        BaseAI enemy = null;

        foreach(Collider col in cols)
        {
            if(col.gameObject.tag == "AI")
            {
                enemy = col.gameObject.GetComponent<BaseAI>();
                break;
            }
        }

        if (!enemy)
            return;

        if(enemy.blockDelay != 0)
        {
            enemy.DoDamage(300);
        }
        else
        {
            enemy.DoDamage(dmg);
        }
    }

    public void DoSimpleDamage(float dmg)
    {
        health -= dmg;
    }

    public void DoDamage(float dmg, BaseAI bai = null)
    {
        if(blockTimer != 0 && blockTimer < .5f)
        {
            if (bai)
            {
                bai.Blocked();
                audioGeneral.PlayAudio(swordBlockAud);
            }
            else
            {
                health -= dmg;
                bloodScreenImg.color = new Color(1, 1, 1, .45f);
                audioGeneral.PlayAudio(hurtAud);
            }
        }
        else
        {
            health -= dmg;
            bloodScreenImg.color = new Color(1,1,1,.45f);
            audioGeneral.PlayAudio(hurtAud);

            if (bai)
            {
                StartCoroutine(CameraShake(.3f));
            }
        }
    }

    IEnumerator CameraShake(float time)
    {
        controller.enabled = false;
        hurt = true;

        Vector3 initialCamPos = bobCam.transform.localPosition;

        Vector3 randomPos = new Vector3(Random.Range(initialCamPos.x - .1f, initialCamPos.x + .1f),
            Random.Range(initialCamPos.y - .1f, initialCamPos.y + .1f),
            0);

        while(time > 0)
        {

            if((bobCam.transform.localPosition - randomPos).magnitude < .02f)
            {
                randomPos = new Vector3(Random.Range(initialCamPos.x - .1f, initialCamPos.x + .1f),
            Random.Range(initialCamPos.y - .1f, initialCamPos.y + .1f),
            0);
            }

            bobCam.transform.localPosition = Vector3.Lerp(bobCam.transform.localPosition, randomPos, deltaTime * 25);

            time -= deltaTime;
            yield return null;
        }

        while((bobCam.transform.localPosition - initialCamPos).magnitude > .02f)
        {
            bobCam.transform.localPosition = Vector3.Lerp(bobCam.transform.localPosition, initialCamPos, deltaTime * 25);
        }

        bobCam.transform.localPosition = initialCamPos;

        hurt = false;
        controller.enabled = true;
        controller.mouseLook.Init(transform, controller.cam.transform);

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Wall")
        {
            if((jumped || ControlFreak2.CF2Input.GetKey(KeyCode.Space)) && !climbing)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                climbing = true;
                acrobaticsPivot.SetActive(true);
                bobCam.SetActive(false);
                anim.SetTrigger("Climb");
                hasBeenInAir = false;
                jumped = false;
            }
        }
    }

    public void EndClimb()
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        climbing = false;
        acrobaticsPivot.SetActive(false);
        bobCam.SetActive(true);
        transform.position = acrobaticsPivot.transform.position;
        acrobaticsPivot.transform.localPosition = Vector3.zero;
        bobCam.transform.localRotation = acrobaticsCamera.transform.localRotation;
        controller.mouseLook.Init(transform, controller.cam.transform);

    }

    public void ChangeSkill(string skill)
    {
        skillEquipped = skill;

        if (skill != "Gun" && skill != "Crossbow")
        {
            SkillBase selectedSkill = new SkillBase();

            foreach (SkillBase skillObj in skills)
            {
                if (skillObj.powerName == skill)
                {
                    selectedSkill = skillObj;
                }
            }

            activeSkill = selectedSkill;

        }
        else
        {
            activeSkill = null;
        }


        StartCoroutine(reequip());        

    }

    public void ChangeSkillUI(string skill)
    {
        skillTX.text = skill;
    }

    IEnumerator reequip()
    {

        while (!controller.enabled)
        {
            yield return null;
        }

        if ((hasSword && !rightEquiped) || !hasSword)
        {
            handsCharAnim.SetBool("RightEquiped", false);
        }
        handsCharAnim.SetBool("LeftEquiped", false);


        yield return new WaitForSeconds(1f);

        bool foundWeapon = false;

        if(skillEquipped != "Gun")
        {
            gun.SetActive(false);
        }
        else
        {
            gun.SetActive(true);
            foundWeapon = true;
        }

        if (skillEquipped != "Crossbow")
        {
            crossbow.SetActive(false);
        }
        else
        {
            crossbow.SetActive(true);
            foundWeapon = true;
        }

        if (!foundWeapon)
        {
            if(skills.Count > 0)
            {
                foundWeapon = true;
            }
        }

        if (foundWeapon)
        {
            leftEquiped = true;

            handsCharAnim.SetBool("LeftEquiped", true);
        }
        else
        {
            skillEquipped = "";
            activeSkill = null;
        }

        if (hasSword && hasSword)
        {
            rightEquiped = true;
            handsCharAnim.SetBool("RightEquiped", true);
        }

    }

    public void RefreshMission()
    {
        missionPoint = FindObjectOfType<MissionPoint>();
    }

    public void CancelPower()
    {
        if(activeSkill && activeSkill.isHoldable)
        {
            activeSkill.CancelPower();
        }
    }


    public void AddPower(GameObject power)
    {
        skills.Add(power.GetComponent<SkillBase>());
    }
    private void OnApplicationQuit()
    {
        SetVignette(0);
    }
}
