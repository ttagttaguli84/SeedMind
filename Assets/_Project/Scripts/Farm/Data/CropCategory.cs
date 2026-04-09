namespace SeedMind.Farm.Data
{
    /// <summary>
    /// 작물 분류 enum. 가공 유형 결정에 사용.
    /// -> see docs/pipeline/data-pipeline.md 섹션 2.1
    /// </summary>
    public enum CropCategory
    {
        Vegetable,      // 채소 (감자, 당근, 옥수수, 호박, 겨울무, 시금치)
        Fruit,          // 과일 (수박)
        FruitVegetable, // 과채 (토마토, 딸기)
        Fungi,          // 균류 (표고버섯)
        Flower,         // 꽃 (해바라기)
        Special         // 특수
    }
}
