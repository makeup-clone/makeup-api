using Application.Models;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public partial class CategoryMapper
{
    [MapProperty(nameof(CategoryEntity.Children), nameof(CategoryModel.Children))]
    public partial CategoryModel CategoryEntityToCategoryModel(CategoryEntity entity);

    // Допоміжний явний метод для колекції Children
    public partial List<CategoryModel> MapChildren(ICollection<CategoryEntity>? children);


    [MapperIgnoreTarget(nameof(CategoryEntity.Children))]
    public partial CategoryEntity CategoryModelToCategoryEntity(CategoryModel model);
}