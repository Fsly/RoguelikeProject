using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    public float smoothing = 1;
    public float restTime = 1;
    public AudioClip chop1Audio;
    public AudioClip chop2Audio;
    public AudioClip step1Audio;
    public AudioClip step2Audio;

    public AudioClip soda1Audio;
    public AudioClip soda2Audio;
    public AudioClip fruit1Audio;
    public AudioClip fruit2Audio;

    private float restTimer = 0;
    [HideInInspector] public Vector2 targetPos = new Vector2(1, 1);
    private new Rigidbody2D rigidbody;
    private new BoxCollider2D collider;
    private Animator animator;
    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //使用刚体控制移动
        rigidbody.MovePosition(Vector2.Lerp(transform.position, targetPos, smoothing * Time.deltaTime));

        //游戏结束无法移动
        if (GameManager.Instance.food <= 0 || GameManager.Instance.isEnd == true) return;

        //休息计时器
        restTimer += Time.deltaTime;
        if (restTimer < restTime) return;

        //获取输入
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h > 0)
        {
            v = 0;
        }

        if (h != 0 || v != 0)
        {
            GameManager.Instance.ReduceFood(1);

            //检测，启用行为会更新，禁用行为不会更新。
            collider.enabled = false;
            RaycastHit2D hit = Physics2D.Linecast(targetPos, targetPos + new Vector2(h, v));
            collider.enabled = true;
            //空地
            if (hit.transform == null)
            {
                targetPos += new Vector2(h, v);
                AudioManager.Instance.RandomPlay(step1Audio, step2Audio);
            }
            else
            {
                switch (hit.collider.tag)
                {
                    case "OutWall":
                        break;
                    case "Wall":
                        animator.SetTrigger("Attack");
                        AudioManager.Instance.RandomPlay(chop1Audio, chop2Audio);
                        hit.collider.SendMessage("TakeDamage");
                        break;
                    case "Food":
                        GameManager.Instance.AddFood(10);
                        targetPos += new Vector2(h, v);
                        AudioManager.Instance.RandomPlay(step1Audio, step2Audio);
                        Destroy(hit.transform.gameObject);
                        AudioManager.Instance.RandomPlay(fruit1Audio, fruit2Audio);
                        break;
                    case "Soda":
                        GameManager.Instance.AddFood(20);
                        targetPos += new Vector2(h, v);
                        AudioManager.Instance.RandomPlay(step1Audio, step2Audio);
                        Destroy(hit.transform.gameObject);
                        AudioManager.Instance.RandomPlay(soda1Audio, soda2Audio);
                        break;
                    case "Enemy":
                        break;
                }
            }
            GameManager.Instance.OnPlayerMove();

            restTimer = 0;//不管是攻击还是移动 都需要休息
        }
    }

    public void TakeDamage(int lossFood)
    {
        GameManager.Instance.ReduceFood(lossFood);
        animator.SetTrigger("Damage");
    }
}
