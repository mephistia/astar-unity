using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class MazeGenerator : MonoBehaviour
{
    // direções possíveis de vizinhos
    static readonly Vector2Int[] DIRECTIONS = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1)
    };

    public Transform mouse;

    const int mazeDepth = 19;
    const int mazeWidth = 22;

    int[,] maze = new int[19, 22] {
{ 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
{ 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 },
{ 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1 },
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1 },
{ 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1 },
{ 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1 },
{ 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1 },
{ 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1 },
{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 }
};

    public List<GameObject> tileset = new List<GameObject>();

    float tileWidth = 3.0f;
    float tileDepth = 3.0f;

    float xi = -25.0f;
    float zi = 25.0f;

    // grid específica do grafo
    Node[,] nodeGrid = new Node[mazeDepth, mazeWidth];

    List<Node> path = new List<Node>();

    Node start, end;

    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i<mazeDepth; i++) //z
        {
            for (int j=0; j<mazeWidth; j++) //x
            {
                int mazeValue = maze[i, j];
                GameObject tilePrefab = tileset[mazeValue];
                Vector3 p = tilePrefab.transform.position;
                p.x = xi + j * tileWidth;
                p.z = zi - i * tileDepth;
                // cria o nodo na grid
                nodeGrid[i, j] = new Node((mazeValue == 1), p, i, j);


                GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;
                
            }
        }

        // pontos de início e fim do caminho
        start = nodeGrid[0, 0];
        end = nodeGrid[mazeDepth - 1, mazeWidth - 2];

        FindPath(start, end);
    }

    void FindPath(Node startNode, Node endNode)
    {
        // SimplePriorityQueue -> pacote NuGet de fila de prioridade (o float menor fica na frente)
        SimplePriorityQueue<Node, float> openNodes = new SimplePriorityQueue<Node>(); // "frontier" no Red Blob, nodos não visitados
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>(); // custo até esse nodo        

        openNodes.Enqueue(startNode, 0); // Adiciona na queue
        startNode.cameFrom = start; // primeiro nodo "veio dele mesmo"
        costSoFar[start] = 0;

        // enquanto existem nodos "abertos"
        while (openNodes.Count > 0)
        {
            // recupera o nodo não-visitado (e remove da fila)
            Node current = openNodes.Dequeue();
            Debug.Log("Visitando nodo em " + current.gridX + ", " + current.gridY);

            // finaliza o loop se chegou no final
            if (current == endNode)
            {
                Debug.Log("Caminho encontrado!");
                // refaz o caminho de acordo com os nodos "pai"
                RetracePath(startNode, endNode);
                return;
            }

            // para cada vizinho
            foreach (Node next in GetNeighbours(current))
            {
                float newCost = costSoFar[current] + GetCost(next, endNode);

                // se ainda não foi visitado, ou se o novo custo for menor (encontrou um caminho melhor)
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    // atualiza/seta o custo
                    costSoFar[next] = newCost;

                    // seta prioridade
                    float priority = newCost + GetHeuristicCost(next, endNode);

                    // adiciona nos abertos (possível caminho)
                    openNodes.Enqueue(next, priority);

                    // atualiza o nodo "pai"
                    next.cameFrom = current;                           
                }
            }

        }

        Debug.LogError("CAMINHO NÃO ENCONTRADO!!");
    }

    private float GetCost(Node a, Node b)
    {
        // sem custo de verdade...
        return 1;
    }

    private float GetHeuristicCost(Node a, Node b)
    {
        // "Y" nessa grid é o Z
        return Mathf.Abs(a.position.x - b.position.x) + Mathf.Abs(a.position.z - b.position.z);
    }

    private List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        // pega os nodos em volta na grid
        foreach(Vector2Int direction in DIRECTIONS)
        {
            int realX = direction.x + node.gridX;
            int realY = direction.y + node.gridY;

            if (IsInsideGrid(realX, realY) && !nodeGrid[realX, realY].isWall)
            {
                neighbours.Add(nodeGrid[realX, realY]);
            }
        }

        return neighbours;

    }
    
    private bool IsInsideGrid(int x, int y)
    {
        if (x >= 0 && x < mazeDepth)
        {
            if (y >= 0 && y < mazeWidth)
                return true;
        }

        return false;

    }

    void RetracePath(Node start, Node end)
    {
        // retomar o caminho do fim para o início
        Node current = end;
        Debug.Log("Retraçando caminho...");

        while (current != start)
        {
            Debug.Log("Caminho: " + current.gridX + ", " + current.gridY);
            path.Add(current);
            current = current.cameFrom;
        }

        // reverter, caminho do início para o fim
        path.Reverse();


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // desenha os cubos nas posições do caminho encontrado
        foreach (Node n in path)
        {
            Gizmos.DrawCube(n.position, Vector3.one * 2);
        }
    }

}
