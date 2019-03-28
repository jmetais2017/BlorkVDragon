using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NewShooter : MonoBehaviour
{
    //La cible des tirs
    public GameObject target;

    //Le modèle de projectile
    public GameObject projectile;

    //UI
    public Text countText;
    public Text timeText;
    public Text endText;

    //Réglage de l'offset
    public Vector3 origine; //(coordonnées locales)
    public Vector3 objectif; //(coordonnées locales)

    //Paramètres du jeu
    public int hitsNeeded;
    public float maxDuration;

    //Paramètres de tir
    public int T; //nb de frames de la période de shoot
    private int t = 0;  //current frame
    public float speed; //vitesse du projectile
    public float seuilContact; //rayon de destruction au contact de la cible
    private float seuilDestruction = 2.0f; //rayon de destruction vers l'infini
    private float lastHit = -1.0f; //timing du dernier hit
    public float display; //durée d'affichage de l'aura d'impact

    //Ajout d'une composante aléatoire dans la précision des tirs
    private Vector3 newOrigine;
    private Vector3 newObjectif;
    private float precision = 0.04f;

    //État du shooter
    [SerializeField]
    private bool isShooting = false;
    private bool wasShooting = false;

    //Détection des objets 3D
    [SerializeField]
    private bool shooterTracked = false;
    [SerializeField]
    private bool targetTracked = false;

    //État du jeu
    [SerializeField]
    private int hitCount;
    private bool gameEnded = false;
    private float runTime;
    private float startTimer;


    //Le projectile
    public class Bullet
    {
        public Vector3 move;
        public Vector3 destination;
        public GameObject body;

        public void destroyBullet() { Destroy(body); }
        public Bullet(Vector3 mov, Vector3 des, GameObject bod) { move = mov; destination = des; body = bod; }

    }

    //On stocke la liste des projectiles présents dans la scène
    private List<Bullet> projectiles = new List<Bullet>();


    // Start is called before the first frame update
    void Start()
    {
        startTimer = Time.time;
        runTime = 0.0f;
        timeText.text = runTime.ToString();

        hitCount = 0;
        countText.text = "Hits : " + hitCount.ToString();

        endText.text = " ";
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (!gameEnded)
        {
            //Mise à jour du chrono
            float duration = Time.time - startTimer;
            runTime = ((int)((duration) * 100.0f)) / 100.0f; //Affichage au centième de seconde
            timeText.text = runTime.ToString();

            //Fin du jeu?
            if (duration > maxDuration)
            {
                endText.color = new Color(0.5f, 0.0f, 0.5f, 1.0f);
                endText.text = "Victoire du blork !";
                gameEnded = true;
            }
            else if (hitCount == hitsNeeded)
            {
                endText.color = Color.blue;
                endText.text = "Victoire du dragon !";
                gameEnded = true;
            }

            //Touché?
            if(duration - lastHit < display)
            {
                target.GetComponentInChildren<Renderer>().enabled = true;
            }
            else
            {
                target.GetComponentInChildren<Renderer>().enabled = false;
            }

            //On actualise l'état du shooter
            shooterTracked = this.gameObject.GetComponentInChildren<DefaultTrackableEventHandler>().isTracked;
            targetTracked = target.GetComponentInChildren<DefaultTrackableEventHandler>().isTracked;
            isShooting = shooterTracked && targetTracked;

            //Gestion du tir
            if (isShooting)
            {
                if (!wasShooting) //Si on commence à shooter, on lance un nouveau cycle
                {
                    t = 0;
                }

                Shoot(t);
            }
            else
            {
                if (wasShooting) //Si on arrête de shooter, on détruit tous les projectiles
                {
                    while (projectiles.Count > 0)
                    {
                        projectiles[0].destroyBullet();
                        projectiles.RemoveAt(0);
                    }

                }
            }

            //On actualise l'état du cycle de shoot
            t++;
            wasShooting = isShooting;
        }
        else
        {
            target.GetComponentInChildren<Renderer>().enabled = false;
        }
    }

    private void Shoot(int t)
    {
        Debug.Log(projectiles.Count);

        if (t % T == 0)
        {
            //Si on est au début d'une période, on crée un nouveau projectile puis on l'ajoute à la liste
            
            newObjectif = objectif + new Vector3(Random.value, Random.value, Random.value) * precision;
            //newOrigine = origine + new Vector3(Random.value, Random.value, Random.value) * precision;
            newOrigine = origine;
            GameObject body = GameObject.Instantiate(projectile, this.transform.position + newOrigine, Quaternion.identity, this.transform);
            Vector3 bulletDestination = newObjectif + target.transform.position;
            Bullet newProj = new Bullet(bulletDestination - this.transform.position - newOrigine, bulletDestination, body);
            projectiles.Add(newProj);
        }
        
        List<Bullet> toDestroyB = new List<Bullet>();
        List<int> indices = new List<int>();

        //On actualise les positions de tous les projectiles de la liste
        for (int i = 0; i < projectiles.Count; i++)
        {

            Vector3 distance = (projectiles[i].destination - projectiles[i].body.transform.position);
            if (distance.magnitude > seuilContact && distance.magnitude < seuilDestruction)
            {
                projectiles[i].body.transform.position += Vector3.Normalize(projectiles[i].move) * speed;
            }
            else
            {
                toDestroyB.Add(projectiles[i]);
                indices.Add(i);

                if (distance.magnitude < seuilContact)
                {
                    hitCount++;
                    countText.text = "Hits : " + hitCount.ToString();
                    lastHit = Time.time;
                }
            }
        }

        //On détruits ceux arrivés à destination ou sortis du champ de vision
        for (int j = 0; j < toDestroyB.Count; j++)
        {
            toDestroyB[j].destroyBullet();
            projectiles.RemoveAt(indices[j]);
        }
    }

}
