using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	#region Properties
	public float playerSpeed;
	public float jumpingSpeed;
	public Transform firePoint;
	public GameObject bulletObject;
	public float hurtCounter;
	public float fireCounter;

	#region Ground check properties
	public Transform groundCheck;
	public float groundCheckRadius;
	public LayerMask groundLayer;
	#endregion

	#endregion

	#region Private
	private Rigidbody2D playerRigidbody;
	private Animator playerAnimator;
	private bool isPlayerOnGround;
	private float vHurtCounter;
	private float vFireCounter;
	private bool facingRight;
	public int bulletsAmount = 10;
	private int bulletIndex;
	private bool handlingAnimation = false;
	private WaitForSeconds wait;

	#region Animation Hash ID's
	private readonly int playerSpeedID = Animator.StringToHash("PlayerSpeed");
	private readonly int onGroundID = Animator.StringToHash("OnGround");
	private readonly int teleporID = Animator.StringToHash("Teleport");
	private readonly int hurtID = Animator.StringToHash("Hurt");
	private readonly int fireID = Animator.StringToHash("Fire");
	private readonly int fireOnAirID = Animator.StringToHash("FireOnAir");
	private readonly int isFiringID = Animator.StringToHash("IsFiring");
	private readonly int skillAttackID = Animator.StringToHash("SkillAttack");
	#endregion

	#region Animation State Hash ID's
	//private readonly int walkingStateID = Animator.StringToHash("Base Layer.Walking");
	#endregion
	#endregion

	// Start is called before the first frame update
	void Start()
    {
		playerRigidbody = GetComponent<Rigidbody2D>();
		playerAnimator = GetComponent<Animator>();
		facingRight = true;
		wait = new WaitForSeconds(1.5f);

		BulletPool.bulletPoolInstance.totalBulletsInPool = bulletsAmount;
	}

    // Update is called once per frame
    void Update()
    {
		isPlayerOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);		

		if (vHurtCounter <= 0f)
		{
			#region Horizontal Movement
			if (Input.GetAxisRaw("Horizontal") > 0f)
			{
				// Moving Forward
				playerRigidbody.velocity = new Vector3(playerSpeed, playerRigidbody.velocity.y, 0f);

				// Flip Sprite on X axis
				if (!facingRight)
				{
					FlipPlayer();
				}
			}
			else if (Input.GetAxisRaw("Horizontal") < 0f)
			{
				// Moving Forward
				playerRigidbody.velocity = new Vector3(-playerSpeed, playerRigidbody.velocity.y, 0f);

				// Flip Sprite on X axis
				if (facingRight)
				{
					FlipPlayer();
				}
			}
			else
			{
				playerRigidbody.velocity = new Vector3(0f, playerRigidbody.velocity.y, 0f);
			}
			#endregion

			#region Vertical Movement (Jump)
			if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)) && isPlayerOnGround)
			{
				playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, jumpingSpeed, 0f);
			}
			#endregion
		}
		else
		{
			vHurtCounter -= Time.deltaTime;
		}

		AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

		// Always checking if player on Ground or not
		playerAnimator.SetBool(onGroundID, isPlayerOnGround);
		// Always setting the Player Speed to the Animator - Idle if Horizontal PlayerSpeed < 0.05f
		playerAnimator.SetFloat(playerSpeedID, Mathf.Abs(playerRigidbody.velocity.x));
		// Always setting the Player as NOT Shooting unless the player is shooting
		if (vFireCounter <= 0f)
		{
			playerAnimator.SetBool(isFiringID, false);
		}
		else
		{
			vFireCounter -= Time.deltaTime;
		}

		#region Teleport
		if (Input.GetKeyDown(KeyCode.T) && isPlayerOnGround)
		{
			if (Mathf.Abs(playerRigidbody.velocity.x) < 0.05f)
				playerAnimator.SetTrigger(teleporID);
		}
		#endregion

		#region Hurt
		else if (Input.GetKeyDown(KeyCode.H))
		{
			playerAnimator.SetTrigger(hurtID);
			if (Mathf.Abs(playerRigidbody.velocity.x) != 0.05f)
			{
				playerRigidbody.velocity = new Vector3(0f, playerRigidbody.velocity.y, 0f);
				vHurtCounter = hurtCounter;
			}
			
		}
		#endregion

		#region Shooting
		else if (Input.GetKeyDown(KeyCode.V))
		{
			playerAnimator.SetBool(isFiringID, true);
			if (isPlayerOnGround)
			{
				firePoint.position = new Vector3(firePoint.position.x, transform.position.y - 0.04f, firePoint.position.z);
				if (Math.Abs(playerRigidbody.velocity.x) < 0.05f)
				{
					playerAnimator.SetTrigger(fireID);
				}
				else
				{
					playerAnimator.SetBool(isFiringID, true);
					vFireCounter = fireCounter;
				}	
			}
			else
			{
				firePoint.position = new Vector3(firePoint.position.x, transform.position.y + 0.22f, firePoint.position.z);
				playerAnimator.SetTrigger(fireOnAirID);
			}

			Shoot();
		}
		#endregion

		#region Skill Attack
		else if (Input.GetKeyDown(KeyCode.S))
		{
			if (isPlayerOnGround && Math.Abs(playerRigidbody.velocity.x) == 0f && Math.Abs(playerRigidbody.velocity.y) == 0f)
			{
				//firePoint.position = new Vector3(firePoint.position.x, transform.position.y - 0.04f, firePoint.position.z);
				playerAnimator.SetTrigger(skillAttackID);
			}

			Shoot();
		}
		#endregion
	}

	private void FlipPlayer()
	{
		facingRight = !facingRight; // FacingRight becomes the opposite of the current value.
		transform.Rotate(0f, 180f, 0f);
	}

	private void Shoot()
	{
		bulletIndex = bulletIndex % bulletsAmount;
		GameObject bullet = BulletPool.bulletPoolInstance.GetBullet(bulletIndex++);
		bullet.transform.position = firePoint.position;
		bullet.transform.rotation = firePoint.rotation;
		bullet.SetActive(true);
	}

	private IEnumerator CoWait()
	{
		Debug.Log($"Coroutine started");
		handlingAnimation = true;
		// process pre-yield
		yield return wait;
		// process post-yield
		handlingAnimation = false;
		Debug.Log($"Coroutine ended");
	}
}
