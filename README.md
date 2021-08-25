# Repository
This repository contains the code to build services with common codes in the shortest possible time.

Allows you to configure MongoDB and a generic repository to manage the database in MontoDB

Allows you to configure SqlServer and a generic repository to manage the database in SqlServer

### `appsettings.json` configuration

The file path and other settings can be read from JSON configuration if desired.

In `appsettings.json` add a `"MongoDbSettings", "ServiceSettings"` properties:

```json
{
   "MongoDbSettings": {
    "Host": "localhost",
    "Port": "27017"
  },
  "ServiceSettings": {
    "ServiceName": "Catalog",
    "Authority": "https://localhost:5003"
  }
}
```

And then pass the configuration section to the next methods:

```csharp
services.AddMongo()
        .AddMongoRepository<Item>("{{YourTableName}}");
```

Or

```csharp
services.AddSqlRepository<Item, {{YourDbContext}}>();
```

Your model must inherit from Entity class

```csharp
public class Item : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}
```

Example of a controller using dependency injection services

```csharp
[Route("[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private const string AdminRole = "Admin";
    private readonly IRepository<Item> _itemsRepository;

    public ItemsController(IRepository<Item> itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }

    [HttpGet]
    [Authorize(Policies.Read)]
    public async Task<IEnumerable<ItemDto>> GetAsync()
    {
        var items = (await _itemsRepository.GetAllAsync()).Select(item => item.AsDto());
        return items;
    }

    [HttpGet("{id}")]
    [Authorize(Policies.Read)]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await _itemsRepository.GetAsync(id);

        if(item == null)
        {
            return NotFound();
        }

        return item.AsDto();
    }

    [HttpPost]
    [Authorize(Policies.Write)]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
    {
        var item =  new Item
        {
            Name = createItemDto.Name,
            Description = createItemDto.Description,
            Price = createItemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await _itemsRepository.CreateAsync(item);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id}, item);
    }

    [HttpPut("{id}")]
    [Authorize(Policies.Write)]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
    {
        var existingItem = await _itemsRepository.GetAsync(id);

        if(existingItem == null)
        {
            NotFound();
        }

        existingItem.Name = updateItemDto.Name;
        existingItem.Description = updateItemDto.Description;
        existingItem.Price = updateItemDto.Price;

        await _itemsRepository.UpdateAsync(existingItem);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policies.Write)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var existingItem = await _itemsRepository.GetAsync(id);

        if (existingItem == null)
        {
            NotFound();
        }

        await _itemsRepository.RemoveAsync(existingItem.Id);
        return NoContent();
    }
}
```
