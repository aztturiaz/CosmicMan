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
	private float hurtCounter;
	private bool facingRight;
	public int bulletsAmount = 10;
	private int bulletIndex;
	#endregion



	// Start is called before the first frame update
	void Start()
    {
		playerRigidbody = GetComponent<Rigidbody2D>();
		playerAnimator = GetComponent<Animator>();
		facingRight = true;
	}

    // Update is called once per frame
    void Update()
    {
		isPlayerOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);		

		if (hurtCounter <= 0f)
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
			hurtCounter -= Time.deltaTime;
		}

		// Always checking if player on Ground or not
		playerAnimator.SetBool("OnGround", isPlayerOnGround);
		// Always setting the Player Speed to the Animator - Idle if Horizontal PlayerSpeed < 0.05f
		playerAnimator.SetFloat("PlayerSpeed", Mathf.Abs(playerRigidbody.velocity.x));
		// Always setting the Player as NOT Shooting
		playerAnimator.SetBool("IsFiring", false);

		#region Teleport
		if (Input.GetKeyDown(KeyCode.T) && isPlayerOnGround)
		{
			if (Mathf.Abs(playerRigidbody.velocity.x) < 0.05f)
				playerAnimator.SetTrigger("Teleport");
		}
		#endregion

		#region Hurt
		else if (Input.GetKeyDown(KeyCode.H))
		{
			playerAnimator.SetTrigger("Hurt");
			if (Mathf.Abs(playerRigidbody.velocity.x) != 0.05f)
			{
				Debug.Log($"Hurt animation started. Player velocity = (X= {playerRigidbody.velocity.x}, Y={playerRigidbody.velocity.y})");
				playerRigidbody.velocity = new Vector3(0f, playerRigidbody.velocity.y, 0f);
				hurtCounter = 0.25f;
			}
			
		}
		#endregion

		#region Shooting
		else if (/*Input.GetButtonDown("Fire1") || */Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.V))
		{
			playerAnimator.SetBool("IsFiring", true);
			if (isPlayerOnGround)
			{
				firePoint.position = new Vector3(firePoint.position.x, transform.position.y - 0.04f, firePoint.position.z);
				playerAnimator.SetTrigger("Fire");
			}
			else
			{
				firePoint.position = new Vector3(firePoint.position.x, transform.position.y + 0.22f, firePoint.position.z);
				playerAnimator.SetTrigger("FireOnAir");
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
		/*for (int i = 0; i < bulletsAmount + 1; i++)
		{*/
		bulletIndex = bulletIndex % bulletsAmount;
			GameObject bullet = BulletPool.bulletPoolInstance.GetBullet(bulletIndex++);
			bullet.transform.position = firePoint.position;
			bullet.transform.rotation = firePoint.rotation;
			bullet.SetActive(true);
		//}
		//Instantiate(bulletObject, firePoint.position, firePoint.rotation);
	}

	private IEnumerator CoWait()
	{
		Debug.Log($"Coroutine started");
		//beingHandled = true;
		// process pre-yield
		yield return new WaitForSeconds(5.0f);
		// process post-yield
		//beingHandled = false;
		Debug.Log($"Coroutine ended");
	}
}
