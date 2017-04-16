/* 
TODO
    GAMEPLAY
    - Skills: Trou noir = détruit une planète
    - Skills: Conquer = Change la couleur d'une planète
    - Skills: Teleport = Attaque une planète sans lien
    - Skills: Firewall = Protège une planète 3 tours
    
    EDITEUR
    - 

    UI
    - Reduire la largeur de la selection de planète
    - Faire un appui long pour voir la zone de reach apparaitre (appui court = sélection)
    - Double tap sur l'écran nous amène au zoom maximal.
    - Faire un feedback quand le joueur essaie de scroller mais qu'il a atteint le borde de la zone de jeu (genre un border blanc proportionel a la distance scrollée qui apparait)

    GENERATION
    - Placer toujours les players le plus loin possible les uns des autres
    - Interdire les planètes trop proche
    - Interdire les planètes trop éloignées
    - Positionnement moins "carré"

    BUGS
    - BUG: Il y a souvent deux planètes l'une sur l'autre lors de la génération
    - BUG: Il ne faut pas créer d'unité lorsque l'on attaque une planète vide (2 sur 0 donne 1 et 1)
    - BUG: Les joueurs morts jouent quand même les salaudSs!
    - BUG: Le script de caméra devrait etre éxécuté avant le script d'UI, mais le panneau script execution order est tout bugué sa mère
    - BUG: Sur PC et sur Mobile, on peut dépasser le zoom maximum pendant jusqu'a un point on ou est recalé sur le bon zoom max
    - BUG: Sur Mobile, L'inertie de scrolling est beaucoup trop sensible et incontrolable (désactivée pour le moment)

    SOUND
    - Sfx de sélection et de création de links





DONE
    - Sortir les players du prefab
    - Pouvoir préciser qui joue en premier
    - Pouvoir se déplacer hors screen avec double touch
    - Avoir des planètes donnant une ressource spéciale à chaque tour
    - Avoir moins de hierarchie dans le prefab pour l'éditer dans le browser
    - Reduire un peu la zone de reach des link
    - Plein de trucs trop oufs
*/
