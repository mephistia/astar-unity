# A* pathfinding na Unity

Scripts para o A*:
- Node
- MazeGenerator (editado)

## Node
Classe com a estrutura do nodo. Salva o nodo "pai" do caminho, de onde ele veio, além da posição no mundo e na grid.

## MazeGenerator
Gera o labirinto e a grid para o grafo. Encontra um caminho a partir de posições de início e fim. Desenha os cubos que representam cada posição do caminho encontrado. Mais explicações nos comentários do código (método principal de A* é o `FindPath`)

Baseado fortemente na explicação e código de [Red Blob Games](https://www.redblobgames.com/pathfinding/a-star/introduction.html) :)