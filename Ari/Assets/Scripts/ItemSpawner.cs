using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab; // Prefab do item para spawnar
    public int maxItemsInScene = 5; // Máximo de itens na cena
    public float spawnInterval = 10f; // Intervalo entre spawns (segundos)
    public float spawnHeight = 2f; // Altura acima do chão para spawnar
    
    [Header("Spawn Area")]
    public Transform leftBoundary; // Limite esquerdo da área de spawn
    public Transform rightBoundary; // Limite direito da área de spawn
    public float maxSpawnHeight = 50f; // Altura máxima para procurar chão (aumentado para mapas maiores)
    
    [Header("Ground Detection")]
    public LayerMask groundLayerMask = -1; // Layer do chão e plataformas
    public float raycastDistance = 100f; // Distância do raycast para detectar chão (aumentado)
    public bool showDebugRaycasts = true; // Mostrar raycasts no Scene View
    
    private List<GameObject> spawnedItems = new List<GameObject>();
    private float nextSpawnTime;
    
    void Start()
    {
        Debug.Log("ItemSpawner iniciado!");
        
        // Se não tiver boundaries definidas, usa os limites da câmera
        if (leftBoundary == null || rightBoundary == null)
        {
            Debug.Log("Boundaries não definidas, criando automaticamente baseado na câmera...");
            SetCameraBoundaries();
        }
        
        if (itemPrefab == null)
        {
            Debug.LogError("ERRO: Item Prefab não foi definido no ItemSpawner!");
            return;
        }
        
        Debug.Log($"Área de spawn configurada: X de {leftBoundary.position.x} até {rightBoundary.position.x}");
        Debug.Log($"Próximo spawn em {spawnInterval} segundos. Máximo de {maxItemsInScene} itens na cena.");
        
        nextSpawnTime = Time.time + spawnInterval;
        
        // Spawna alguns itens iniciais
        StartCoroutine(SpawnInitialItems());
    }
    
    void Update()
    {
        // Remove itens nulos da lista (caso tenham sido coletados)
        int itemsBeforeCleanup = spawnedItems.Count;
        spawnedItems.RemoveAll(item => item == null);
        int itemsAfterCleanup = spawnedItems.Count;
        
        if (itemsBeforeCleanup != itemsAfterCleanup)
        {
            Debug.Log($"Item coletado! Itens restantes: {itemsAfterCleanup}");
        }
        
        // Verifica se é hora de spawnar um novo item
        if (Time.time >= nextSpawnTime && spawnedItems.Count < maxItemsInScene)
        {
            Debug.Log($"Tentando spawnar item... (Atual: {spawnedItems.Count}/{maxItemsInScene})");
            SpawnItem();
            nextSpawnTime = Time.time + spawnInterval;
            Debug.Log($"Próximo spawn em {spawnInterval} segundos (às {nextSpawnTime:F1})");
        }
    }
    
    void SetCameraBoundaries()
    {
        // Usa coordenadas específicas da área de spawn
        float leftX = -130f;  // Limite esquerdo da área
        float rightX = -30f;  // Limite direito da área
        
        Debug.Log($"Configurando boundaries automáticas: X = {leftX} até X = {rightX}");
        
        // Cria GameObjects temporários para os boundaries
        if (leftBoundary == null)
        {
            GameObject leftBound = new GameObject("LeftBoundary_Auto");
            leftBound.transform.position = new Vector3(leftX, 0, 0);
            leftBoundary = leftBound.transform;
        }
        
        if (rightBoundary == null)
        {
            GameObject rightBound = new GameObject("RightBoundary_Auto");
            rightBound.transform.position = new Vector3(rightX, 0, 0);
            rightBoundary = rightBound.transform;
        }
    }
    
    IEnumerator SpawnInitialItems()
    {
        // Spawna alguns itens iniciais com delay
        int initialItems = Mathf.Min(3, maxItemsInScene);
        
        for (int i = 0; i < initialItems; i++)
        {
            SpawnItem();
            yield return new WaitForSeconds(1f); // Delay entre spawns iniciais
        }
    }
    
    void SpawnItem()
    {
        Vector3 spawnPosition = FindValidSpawnPosition();
        
        if (spawnPosition != Vector3.zero)
        {
            GameObject newItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
            spawnedItems.Add(newItem);
            
            Debug.Log($"Item spawnado em: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("Não foi possível encontrar uma posição válida para spawn!");
        }
    }
    
    Vector3 FindValidSpawnPosition()
    {
        int maxAttempts = 20; // Máximo de tentativas para encontrar uma posição válida
        Debug.Log($"Procurando posição válida para spawn... (Tentativas máximas: {maxAttempts})");
        Debug.Log($"Ground Layer Mask configurado para: {groundLayerMask.value} (binário: {System.Convert.ToString(groundLayerMask.value, 2)})");
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // Gera uma posição X aleatória entre os boundaries
            float randomX = Random.Range(leftBoundary.position.x, rightBoundary.position.x);
            
            // Começa de uma altura alta e faz raycast para baixo
            Vector3 rayStart = new Vector3(randomX, maxSpawnHeight, 0);
            Vector3 rayEnd = new Vector3(randomX, maxSpawnHeight - raycastDistance, 0);
            
            // Mostra o raycast no Scene View para debug
            if (showDebugRaycasts)
            {
                Debug.DrawLine(rayStart, rayEnd, Color.red, 2f);
            }
            
            // Faz raycast para detectar o chão (incluindo tiles)
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, raycastDistance, groundLayerMask);
            
            // Tenta também com OverlapPoint para detectar tiles
            if (hit.collider == null)
            {
                // Testa vários pontos ao longo do raycast para pegar tiles
                for (float testY = maxSpawnHeight; testY > maxSpawnHeight - raycastDistance; testY -= 0.5f)
                {
                    Vector2 testPoint = new Vector2(randomX, testY);
                    Collider2D tileCollider = Physics2D.OverlapPoint(testPoint, groundLayerMask);
                    
                    if (tileCollider != null)
                    {
                        // Simula um hit para tiles
                        hit = new RaycastHit2D();
                        // Não podemos definir diretamente, então fazemos outro raycast mais preciso
                        RaycastHit2D preciseHit = Physics2D.Raycast(
                            new Vector2(randomX, testY + 1f), 
                            Vector2.down, 
                            2f, 
                            groundLayerMask
                        );
                        
                        if (preciseHit.collider != null)
                        {
                            hit = preciseHit;
                            break;
                        }
                        else
                        {
                            // Para tiles, usa a posição do tile
                            Vector3 spawnPos = new Vector3(randomX, testY + spawnHeight, 0);
                            
                            if (!Physics2D.OverlapCircle(spawnPos, 0.5f, groundLayerMask))
                            {
                                Debug.Log($"✓ Posição válida encontrada na tentativa {i + 1} (TILE): {spawnPos}");
                                return spawnPos;
                            }
                        }
                    }
                }
            }
            
            if (hit.collider != null)
            {
                // Posição válida encontrada - spawna acima do chão
                Vector3 spawnPos = new Vector3(randomX, hit.point.y + spawnHeight, 0);
                
                Debug.Log($"Tentativa {i + 1}: Chão encontrado em Y={hit.point.y} (objeto: {hit.collider.name}), spawn em Y={spawnPos.y}");
                
                // Verifica se não há obstáculos na posição de spawn
                if (!Physics2D.OverlapCircle(spawnPos, 0.5f, groundLayerMask))
                {
                    Debug.Log($"✓ Posição válida encontrada na tentativa {i + 1}: {spawnPos}");
                    return spawnPos;
                }
                else
                {
                    Debug.Log($"✗ Tentativa {i + 1}: Posição bloqueada por obstáculo");
                }
            }
            else
            {
                Debug.Log($"✗ Tentativa {i + 1}: Nenhum chão encontrado em X={randomX}");
            }
        }
        
        Debug.LogWarning($"❌ Não foi possível encontrar posição válida após {maxAttempts} tentativas!");
        Debug.LogWarning("💡 DICAS: 1) Verifique se o Ground Layer Mask inclui a layer dos tiles");
        Debug.LogWarning("💡 DICAS: 2) Verifique se os tiles têm Collider2D");
        Debug.LogWarning("💡 DICAS: 3) Tente aumentar o raycastDistance ou maxSpawnHeight");
        return Vector3.zero; // Não encontrou posição válida
    }
    
    // Método público para spawnar item manualmente
    public void ForceSpawnItem()
    {
        if (spawnedItems.Count < maxItemsInScene)
        {
            SpawnItem();
        }
    }
    
    // Método para limpar todos os itens
    public void ClearAllItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        spawnedItems.Clear();
        Debug.Log("Todos os itens foram removidos!");
    }
    
    // Método para debug - mostra status atual
    [ContextMenu("Mostrar Status do Spawner")]
    public void ShowSpawnerStatus()
    {
        Debug.Log("=== STATUS DO ITEM SPAWNER ===");
        Debug.Log($"Prefab definido: {(itemPrefab != null ? itemPrefab.name : "NENHUM")}");
        Debug.Log($"Itens na cena: {spawnedItems.Count}/{maxItemsInScene}");
        Debug.Log($"Próximo spawn em: {(nextSpawnTime - Time.time):F1} segundos");
        Debug.Log($"Boundaries: X={leftBoundary?.position.x:F1} até X={rightBoundary?.position.x:F1}");
        Debug.Log($"Ground Layer Mask: {groundLayerMask.value}");
        Debug.Log("============================");
    }
    
    // Método para testar spawn imediato
    [ContextMenu("Forçar Spawn Agora")]
    public void ForceSpawnNow()
    {
        Debug.Log("Forçando spawn imediato...");
        SpawnItem();
    }
    
    // Método para configurar área de spawn manualmente
    [ContextMenu("Configurar Área de Spawn (-130 a -30)")]
    public void SetCustomSpawnArea()
    {
        SetSpawnArea(-130f, -30f);
    }
    
    // Método público para definir área de spawn
    public void SetSpawnArea(float leftX, float rightX)
    {
        // Remove boundaries antigas se foram criadas automaticamente
        if (leftBoundary != null && leftBoundary.name.Contains("Auto"))
        {
            DestroyImmediate(leftBoundary.gameObject);
        }
        if (rightBoundary != null && rightBoundary.name.Contains("Auto"))
        {
            DestroyImmediate(rightBoundary.gameObject);
        }
        
        // Cria novas boundaries
        GameObject leftBound = new GameObject("LeftBoundary_Custom");
        leftBound.transform.position = new Vector3(leftX, 0, 0);
        leftBoundary = leftBound.transform;
        
        GameObject rightBound = new GameObject("RightBoundary_Custom");
        rightBound.transform.position = new Vector3(rightX, 0, 0);
        rightBoundary = rightBound.transform;
        
        Debug.Log($"Área de spawn configurada: X = {leftX} até X = {rightX}");
    }
    
    // Método para testar detecção de tiles
    [ContextMenu("Testar Detecção de Tiles")]
    public void TestTileDetection()
    {
        Debug.Log("=== TESTE DE DETECÇÃO DE TILES ===");
        
        if (leftBoundary == null || rightBoundary == null)
        {
            Debug.LogError("Boundaries não configuradas!");
            return;
        }
        
        // Testa 5 posições aleatórias
        for (int i = 0; i < 5; i++)
        {
            float testX = Random.Range(leftBoundary.position.x, rightBoundary.position.x);
            Vector3 rayStart = new Vector3(testX, maxSpawnHeight, 0);
            
            Debug.Log($"Teste {i + 1}: Testando em X = {testX}");
            
            // Teste com Raycast
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, raycastDistance, groundLayerMask);
            if (hit.collider != null)
            {
                Debug.Log($"  ✓ Raycast encontrou: {hit.collider.name} em Y = {hit.point.y}");
            }
            else
            {
                Debug.Log($"  ✗ Raycast não encontrou nada");
            }
            
            // Teste com OverlapPoint em várias alturas
            bool foundTile = false;
            for (float testY = maxSpawnHeight; testY > maxSpawnHeight - raycastDistance && !foundTile; testY -= 1f)
            {
                Collider2D tileCollider = Physics2D.OverlapPoint(new Vector2(testX, testY), groundLayerMask);
                if (tileCollider != null)
                {
                    Debug.Log($"  ✓ OverlapPoint encontrou tile: {tileCollider.name} em Y = {testY}");
                    foundTile = true;
                }
            }
            
            if (!foundTile)
            {
                Debug.Log($"  ✗ OverlapPoint não encontrou tiles");
            }
        }
        
        Debug.Log("=== FIM DO TESTE ===");
    }
    
    // Visualização no Scene View
    void OnDrawGizmosSelected()
    {
        if (leftBoundary != null && rightBoundary != null)
        {
            // Desenha a área de spawn
            Gizmos.color = Color.yellow;
            Vector3 leftPos = leftBoundary.position;
            Vector3 rightPos = rightBoundary.position;
            
            // Linha horizontal mostrando a área de spawn
            Gizmos.DrawLine(new Vector3(leftPos.x, maxSpawnHeight, 0), 
                           new Vector3(rightPos.x, maxSpawnHeight, 0));
            
            // Linhas verticais mostrando os limites
            Gizmos.DrawLine(new Vector3(leftPos.x, maxSpawnHeight, 0), 
                           new Vector3(leftPos.x, maxSpawnHeight - raycastDistance, 0));
            Gizmos.DrawLine(new Vector3(rightPos.x, maxSpawnHeight, 0), 
                           new Vector3(rightPos.x, maxSpawnHeight - raycastDistance, 0));
        }
    }
}
