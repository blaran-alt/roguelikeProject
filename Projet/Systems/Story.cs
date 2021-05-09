using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.Systems
{
    public static class Story
    {
        public static readonly string[] TransitionTexts = new string[]
        {
            "Suite à un accident d’origine inconnue, un Hôte peut être plongé dans un comas   sans issue." +
            " Alors que son esprit chute petit à petit dans les abysses, une équipe  d’esprits surentrainés appelés les Réanimateurs sont rapidement dépêchés." +
            " Ils  doivent faire tout ce qui est en leur pouvoir pour ramener l’esprit de l’Hôte et  le garder en vie…",

            "Ok Réanimateur, maintenant que le réseau est rétabli on va pouvoir s’occuper du reste. Commence par rétablir la motricité de l’Hôte. Ce serait dommage qu’on le réanimer sans qu’il puisse bouger. " +
            "Les Bodybuilders sont responsables de la faillite du système. Fais attention à eux, leurs muscles sont extrêmement galbés et ils te lanceront dessus tout ce qu'il trove si tu t'approches trop. " +
            "Combat en le plus possible et trouve les clefs pour passer au prochain système...",

            "Ok Réanimateur, je sais pas ce que t’as fais mais t’as foutu le bordel au niveau de la vision." +
            " T’as tout inversé, ce qui doit être à droite est à gauche et ce qui doit être à gauche… bref t’as compris. Répare moi tout ça en replaçant les capteurs (des boites de couleur) sur les bonnes connections et vite !" +
            "Fait attention aux coquards qui n'aiment pas trop voir des gens traîné dans le coin , ils ne sont pas aussi destructeur que les bodybuilder mais ils peuvent te ralentir énormément."
        };

        public static readonly string[][] levelBeginnigTexts = new string[][]
        {
            new string[]{
                "Les connections entre les neuronnes se font grace a des bornes",
                "Tu les trouveras eparpillees un peu partout sur la carte",
                "Si elles sont activees elle ressemblent a ca : " + (char)10,
                "Sinon elle seront comme ca : " + (char)8,
                "Mets toi dessus et appuie sur E pour en changer l etat,",
                "Mais attention, les coupeurs montent la garde !",
                "Heureusement, tu peux les combattre au corps a corps, ",
                "En avancant sur un ennemi quand il est a cote de toi",
                "Ou en faisant clique gauche sur lui lorsqu il se trouve a porte,",
                "Cette porte apparait lorsque tu le survole avec ta souris.",
                "Les potions que tu ramasses ont plusieurs utilitees :",
                "Elles peuvent te faire regagner de la vie,",
                "Augmenter ta vitesse,",
                "Ou infliger des degats aux ennemis alentours",
                "Tu peux appuyer sur C et V pour faire défiler les potions",
                "Et appuyer sur A pour les utiliser"
            },
            new string[]{
                "En parcourant les salles, tu trouveras 3 clefs.",
                "Ells te permettront d'acceder a la zone suivante",
                "Le passage se trouve dans la derniere salle."
            },
            new string[]{
                "Tu peux pousser une boite en avancant dessus si elle a la place",
                "Tu peux aussi echanger de place avec lorsque tu es a cote",
                "Si tu es vraiment coincé, tu peux la tirer contre toi",
                "Tu ne peux le faire que si elle est a une case de toi",
                "Ou si elle est en diagolnale par rapport a toi",
                "Pour cela, comme d'habitude, appuie sur E."
            }
        };

        public static readonly string[] puzzleSolvedTexts = new string[]
        {
            "Ah c est quand meme mieux comme ca !",
            "Maintenant il faut trouver le passage pour la zone suivante,",
            "Il dervai ressembler a ca : >, mais il est tres bien cache",
            "Tu la peut-être déjà croisé lorsqu'il était ferme : _",
            "Une fois dessus, appuie sur R pour passer à la mission suivante"
        };

    }
}
