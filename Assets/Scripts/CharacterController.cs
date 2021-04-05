using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;

public class CharacterController : MonoBehaviour
{
    private Animator ani;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private TrailRenderer footTrail;
    [SerializeField] private TrailRenderer swordTrail;
    [SerializeField] private Material glowMaterial;
    [SerializeField] private Material trailMaterial;
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject bow;
    [SerializeField] private ParticleSystem switchEffect;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Color fireColor;
    [SerializeField] private Color waterColor;
    [SerializeField] private Color lightColor;
    
    private Coroutine turnRoutine;
    private Vector3 orientation;
    private bool inAction = false;
    private bool usingRootMotion = false;

    private PlayerElementalState playerElementalState = PlayerElementalState.FIRE;

    // Start is called before the first frame update
    void Start()
    {
        ani = GetComponent<Animator>();
        orientation = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dirInput = GetDirectionInput();

        if (!inAction && dirInput.magnitude > 0)
        {
            ani.SetBool("run", true);
            if (!usingRootMotion) transform.position += dirInput * (moveSpeed * Time.deltaTime);
            if (dirInput != orientation)
            {
                orientation = dirInput;
                turnRoutine = StartCoroutine(turnTowards(orientation));
            }
        }
        else
        {
            ani.SetBool("run", false);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!inAction)
            {
                switch (playerElementalState)
                {
                    case PlayerElementalState.FIRE:
                        StartCoroutine(SwordAttack());
                        break;
                    case PlayerElementalState.WATER:
                        StartCoroutine(SwordAttack());
                        break;
                    case PlayerElementalState.LIGHT:
                        StartCoroutine(BowAttack());
                        break;
                }
            }
        } 
        
        if (Input.GetMouseButtonDown(1))
        {
            if (!inAction) StartCoroutine(Hook());
        }

        if (Input.GetKeyDown("space"))
        {
            if (!inAction) StartCoroutine(Roll());
        }

        if (Input.GetKeyDown("q"))
        {
            RotateElementalState(true);
        }
        if (Input.GetKeyDown("e"))
        {
            RotateElementalState(false);
        }
        
        if (Physics.Raycast(transform.position + (Vector3.up * 0.5f), dirInput, 1))
        {
            if (!inAction && dirInput.magnitude > 0)
            {
                StartCoroutine(CollideWithWall(dirInput));
            }
        };
    }

    public void EnableTrail()
    {
        swordTrail.emitting = true;
    }
    
    public void DisableTrail()
    {
        swordTrail.emitting = false;
    }
    
    public void EnableFootTrail()
    {
        footTrail.emitting = true;
    }
    
    public void DisableFootTrail()
    {
        footTrail.emitting = false;
    }

    public void SpawnBowSwitchProjectiles()
    {
        var arrow1 = Instantiate(arrowPrefab);
        arrow1.transform.forward = (transform.forward + Vector3.down).normalized;
        arrow1.transform.position = transform.position + Vector3.up * 1.7f + transform.forward * 0.4f;
        var arrow2 = Instantiate(arrow1);
        arrow2.transform.forward = Quaternion.AngleAxis(30, Vector3.up) * arrow1.transform.forward;
        var arrow3 = Instantiate(arrow1);
        arrow3.transform.forward = Quaternion.AngleAxis(-30, Vector3.up) * arrow1.transform.forward;
        var arrow4 = Instantiate(arrow1);
        arrow4.transform.forward = Quaternion.AngleAxis(60, Vector3.up) * arrow1.transform.forward;
        var arrow5 = Instantiate(arrow1);
        arrow5.transform.forward = Quaternion.AngleAxis(-60, Vector3.up) * arrow1.transform.forward;
    }

    private void TurnToMousePos()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            transform.forward = (new Vector3(hit.point.x, 0, hit.point.z) - transform.position).normalized;
        }
        orientation = transform.forward;
    }

    private Vector3 GetDirectionInput()
    {
        return Quaternion.Euler(0, 45, 0) *
               new Vector3(-Input.GetAxisRaw("Vertical"), 0, Input.GetAxisRaw("Horizontal"))
                   .normalized;
    }

    private IEnumerator turnTowards(Vector3 targetOrientation)
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
        }

        Vector3 cross = Vector3.Cross(transform.forward, targetOrientation);

        int dir = (cross.y < 0) ? -1 : 1;

        while (Mathf.Abs(Vector3.Angle(transform.forward, targetOrientation)) > turnSpeed * Time.deltaTime * 100)
        {
            transform.forward = Quaternion.Euler(0, dir * turnSpeed * Time.deltaTime * 100, 0) * transform.forward;
            yield return null;
        }

        transform.forward = targetOrientation;
        yield return null;
    }

    private IEnumerator SwordAttack()
    {
        TurnToMousePos();
        inAction = true;
        usingRootMotion = true;
        ani.SetTrigger("attack");
        //ani.Play("attack");
        yield return new WaitForSeconds(0.64f);
        inAction = false;
        usingRootMotion = false;
    }
    private IEnumerator Hook()
    {
        TurnToMousePos();
        inAction = true;
        ani.SetTrigger("hook");
        yield return new WaitForSeconds(0.8f);
        inAction = false;
    }
    
    private IEnumerator BowAttack()
    {
        TurnToMousePos();
        inAction = true;
        usingRootMotion = true;
        ani.SetTrigger("bow_shoot");
        bow.GetComponent<Animator>().SetTrigger("pull");
        yield return new WaitForSeconds(0.2f);
        var arrow = Instantiate(arrowPrefab);
        arrow.transform.forward = transform.forward;
        arrow.transform.position = transform.position + Vector3.up * 1.7f + transform.forward * 0.2f;
        yield return new WaitForSeconds(0.44f);
        inAction = false;
        usingRootMotion = false;
    }
    private IEnumerator Roll()
    {
        inAction = true;
        usingRootMotion = true;
        ani.SetTrigger("roll");
        yield return new WaitForSeconds(0.55f);
        inAction = false;
        yield return new WaitForSeconds(0.15f);
        usingRootMotion = false;
    }

    private void RotateElementalState(bool left)
    {
        switch (playerElementalState)
        {
            case PlayerElementalState.FIRE:
                if (left) StartCoroutine(SwitchToLight());
                else StartCoroutine(SwitchToWater());
                break;
            case PlayerElementalState.WATER:
                if (left) StartCoroutine(SwitchToFire());
                else StartCoroutine(SwitchToLight());
                break;
            case PlayerElementalState.LIGHT:
                if (left) StartCoroutine(SwitchToWater());
                else StartCoroutine(SwitchToFire());
                break;
        }
    }

    private IEnumerator SwitchToFire()
    {
        playerElementalState = PlayerElementalState.FIRE;
        glowMaterial.SetColor("Color_ec00cd6084ac4d0b9d49bfeaac867853", fireColor);
        trailMaterial.color = fireColor;
        sword.SetActive(true);
        bow.SetActive(false);
        Debug.Log("Switch to fire");
        switchEffect.startColor = fireColor;
        var switchEffectEmission = switchEffect.emission;
        switchEffectEmission.enabled = true;
        yield return new WaitForSeconds(0.3f);
        switchEffectEmission.enabled = false;
        yield return null;
    }
    
    private IEnumerator SwitchToWater()
    {
        playerElementalState = PlayerElementalState.WATER;
        glowMaterial.SetColor("Color_ec00cd6084ac4d0b9d49bfeaac867853", waterColor);
        trailMaterial.color = waterColor;
        Debug.Log("Switch to water");
        sword.SetActive(true);
        bow.SetActive(false);
        switchEffect.startColor = waterColor;
        var switchEffectEmission = switchEffect.emission;
        switchEffectEmission.enabled = true;
        yield return new WaitForSeconds(0.3f);
        switchEffectEmission.enabled = false;
        yield return null;
    }
    
    private IEnumerator SwitchToLight()
    {
        playerElementalState = PlayerElementalState.LIGHT;
        glowMaterial.SetColor("Color_ec00cd6084ac4d0b9d49bfeaac867853", lightColor);
        trailMaterial.color = lightColor;
        Debug.Log("Switch to ligth");
        bow.SetActive(true);
        sword.SetActive(false);
        switchEffect.startColor = lightColor;
        var switchEffectEmission = switchEffect.emission;
        switchEffectEmission.enabled = true;
        
        inAction = true;
        usingRootMotion = true;
        ani.SetTrigger("switch_bow");
        yield return new WaitForSeconds(0.3f);
        switchEffectEmission.enabled = false;
        yield return new WaitForSeconds(0.5f);
        inAction = false;
        usingRootMotion = false;
    }
    private IEnumerator CollideWithWall(Vector3 direction)
    {
        inAction = true;
        ani.SetBool("collision_front", true);
        yield return new WaitForSeconds(0.2f);
        usingRootMotion = true;
        yield return new WaitUntil(() => GetDirectionInput() != direction);
        inAction = false;
        usingRootMotion = false;
        ani.SetBool("collision_front", false);
    }

    private void OnDestroy()
    {
        glowMaterial.SetColor("Color_ec00cd6084ac4d0b9d49bfeaac867853", fireColor);
        trailMaterial.color = fireColor;
    }
}