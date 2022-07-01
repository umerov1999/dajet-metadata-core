## Методика тестирования справочников

**Контрольный список для тестирования:**
- Код (число)
- Код (строка)
- Без кода
- Без наименования
- Без кода и наименования
- Иерархический (только элементы)
- Иерархический (группы и элементы)
- Подчинённый (один справочник-владелец)
- Подчинённый (много справочников-владельцев)
- Общие реквизиты
- Табличные части
- Предопределённые элементы (начиная с версии 8.3.3)
- Планы обмена (таблица регистрации изменений)

**Условия наличия системных свойств справочника**

| **Имя свойства**     | **Поле СУБД**  | **Тип данных СУБД**  | **Условие наличия**                      |
|----------------------|----------------|----------------------|------------------------------------------|
| **Ссылка**           | _IDRRef        | binary(16)           | **Обязательно**                          |
| **ВерсияДанных**     | _Version       | timestamp            | **Обязательно**                          |
| **ПометкаУдаления**  | _Marked        | binary(1)            | **Обязательно**                          |
| **Код**              | _Code          | nvarchar или numeric | Опционально                              |
| **Наименование**     | _Description   | nvarchar             | Опционально                              |
| **Родитель**         | _ParentIDRRef  | binary(16)           | Иерархический справочник                 |
| **ЭтоГруппа**        | _Folder        | binary(1)            | Иерархия групп и элементов               |
|                      |                | 0x00 - группа        | *Значение в СУБД инвертировано:*         |
|                      |                | 0x01 - элемент       | *нужно для сортировки групп и элементов* |
| **Владелец**         | _OwnerIDRRef   | binary(16)           | Один справочник-владелец                 |
|----------------------|----------------|----------------------|------------------------------------------|
| **Владелец**         | _OwnerID_TYPE  | binary(1) = 0x08     | Много справочников-владельцев            |
|                      | _OwnerID_RTRef | binary(4)            | Код типа справочника-владельца           |
|                      | _OwnerID_RRRef | binary(16)           | Ссылка на элемент справочника-владельца  |
| **Предопределённый** | _PredefinedID  | binary(16) >= 8.3.3    | Предопределённый элемент справочника            |
| **Предопределённый** | _IsMetadata  | binary(1) < 8.3.3    | Если это обычный элемент, то значение равно нулевому UUID   |
| **[Общий реквизит]**   | _Fld<N> | <тип> | Опциональное |

**Последовательность сериализации системных свойств в формат 1С JDTO:**

1. ЭтоГруппа        = IsFolder           - bool (invert)
2. Ссылка           = Ref                - uuid 
3. ПометкаУдаления  = DeletionMark       - bool
4. Владелец         = Owner              - { #type + #value }
5. Родитель         = Parent             - uuid
6. Код              = Code               - string | number
7. Наименование     = Description        - string
8. Предопределённый = PredefinedDataName - string

```C#
// Алгоритм создания системных свойств справочника в зависимости
// от настроек метаданных и порядка сериализации в формат 1С JDTO
private static void ConfigureCatalog(in MetadataCache cache, in Catalog catalog)
{
  if (catalog.IsHierarchical)
  {
    if (catalog.HierarchyType == HierarchyType.Groups)
    {
      ConfigurePropertyЭтоГруппа(catalog);
    }
  }

  ConfigurePropertyСсылка(catalog);
  ConfigurePropertyПометкаУдаления(catalog);

  List<Guid> owners = cache.GetCatalogOwners(catalog.Uuid);

  if (owners != null && owners.Count > 0)
  {
    ConfigurePropertyВладелец(in catalog, in owners);
  }

  if (catalog.IsHierarchical)
  {
    ConfigurePropertyРодитель(catalog);
  }

  if (catalog.CodeLength > 0)
  {
    ConfigurePropertyКод(catalog);
  }

  if (catalog.DescriptionLength > 0)
  {
    ConfigurePropertyНаименование(catalog);
  }

  ConfigurePropertyПредопределённый(catalog);

  ConfigurePropertyВерсияДанных(catalog);
}
```