using ConstantValues;
using Services;
using UnityEngine;

namespace Utilities.ImageCombiner
{
	public interface IImageCombineService : IService
	{
		Sprite CombineImages(int numberOfDots, CellLogic.Cell[] neighbours, Enums.Team team);
		Sprite ClearImage();
	}
}