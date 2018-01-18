﻿using System;
using System.Collections;
using System.Drawing;
using System.Web.UI.WebControls;
using BaiRong.Core;

namespace BaiRong.Model
{
	public enum EImageSizeType
	{
		Square,
		Thumbnail,
		Small,
		Medium,
		Original
	}

	public class EImageSizeTypeUtils
	{
		public static string GetValue(EImageSizeType type)
		{
		    if (type == EImageSizeType.Square)
			{
				return "Square";
			}
		    if (type == EImageSizeType.Thumbnail)
		    {
		        return "Thumbnail";
		    }
		    if (type == EImageSizeType.Small)
		    {
		        return "Small";
		    }
		    if (type == EImageSizeType.Medium)
		    {
		        return "Medium";
		    }
		    if (type == EImageSizeType.Original)
		    {
		        return "Original";
		    }
		    throw new Exception();
		}

		public static string GetText(EImageSizeType type)
		{
		    if (type == EImageSizeType.Square)
			{
				return "矩形";
			}
		    if (type == EImageSizeType.Thumbnail)
		    {
		        return "小图";
		    }
		    if (type == EImageSizeType.Small)
		    {
		        return "较小尺寸";
		    }
		    if (type == EImageSizeType.Medium)
		    {
		        return "中等尺寸";
		    }
		    if (type == EImageSizeType.Original)
		    {
		        return "原始尺寸";
		    }
		    throw new Exception();
		}

		public static EImageSizeType GetEnumType(string typeStr)
		{
			EImageSizeType retval = EImageSizeType.Original;

			if (Equals(EImageSizeType.Square, typeStr))
			{
				retval = EImageSizeType.Square;
			}
			else if (Equals(EImageSizeType.Thumbnail, typeStr))
			{
				retval = EImageSizeType.Thumbnail;
			}
			else if (Equals(EImageSizeType.Small, typeStr))
			{
				retval = EImageSizeType.Small;
			}
			else if (Equals(EImageSizeType.Medium, typeStr))
			{
				retval = EImageSizeType.Medium;
			}
			else if (Equals(EImageSizeType.Original, typeStr))
			{
				retval = EImageSizeType.Original;
			}

			return retval;
		}

		public static string GetAppendix(EImageSizeType type)
		{
			string retval = string.Empty;

			if (type == EImageSizeType.Square)
			{
				retval = "_s";
			}
			else if (type == EImageSizeType.Thumbnail)
			{
				retval = "_t";
			}
			else if (type == EImageSizeType.Small)
			{
				retval = "_m";
			}
			else if (type == EImageSizeType.Medium)
			{
				retval = "_e";
			}
			else if (type == EImageSizeType.Original)
			{
				retval = "_o";
			}

			return retval;
		}

		public const int Size_Max_Medium = 500;
		public const int Size_Max_Small = 240;
		public const int Size_Max_Thumbnail = 100;

        public const int Size_Square = 75;

        public static int GetMaxSize(EImageSizeType type)
        {
            int size = Size_Max_Medium;

            if (type == EImageSizeType.Square)
            {
                size = Size_Square;
            }
            else if (type == EImageSizeType.Thumbnail)
            {
                size = Size_Max_Thumbnail;
            }
            else if (type == EImageSizeType.Small)
            {
                size = Size_Max_Small;
            }

            return size;
        }

		public static ArrayList GetEImageSizeTypeArrayListByLargerInt(int largerInt)
		{
			ArrayList arraylist = new ArrayList();

            arraylist.Add(EImageSizeType.Square);

            if (largerInt > Size_Max_Thumbnail)
            {
                arraylist.Add(EImageSizeType.Thumbnail);
            }

            arraylist.Add(EImageSizeType.Small);

            if (largerInt > Size_Max_Medium)
            {
                arraylist.Add(EImageSizeType.Medium);
            }

			arraylist.Add(EImageSizeType.Original);

			return arraylist;
		}

		private static int GetSmallerInt(Size originalSize, EImageSizeType sizeType, bool isWidthLarger, int largerInt)
		{
			int retval = 0;
			if (isWidthLarger)
			{
				retval = Convert.ToInt32((Convert.ToDouble(largerInt) / Convert.ToDouble(originalSize.Width)) * Convert.ToDouble(originalSize.Height));
			}
			else
			{
				retval = Convert.ToInt32((Convert.ToDouble(largerInt) / Convert.ToDouble(originalSize.Height)) * Convert.ToDouble(originalSize.Width));
			}
			return retval;
		}

		public static Size GetSize(Size originalSize, EImageSizeType sizeType)
		{
			Size size = new Size(originalSize.Width, originalSize.Height);
			bool isWidthLarger = (originalSize.Width > originalSize.Height);
			int largerInt = Math.Max(originalSize.Width, originalSize.Height);

			if (sizeType == EImageSizeType.Medium)
			{
				largerInt = Math.Min(Size_Max_Medium, largerInt);
			}
			else if (sizeType == EImageSizeType.Small)
			{
				largerInt = Math.Min(Size_Max_Small, largerInt);
			}
			else if (sizeType == EImageSizeType.Thumbnail)
			{
				largerInt = Math.Min(Size_Max_Thumbnail, largerInt);
			}
			else if (sizeType == EImageSizeType.Square)
			{
                int squareWidth = Size_Square;
                int squareHeight = Size_Square;
                if (originalSize.Width < Size_Square)
                {
                    squareWidth = originalSize.Width;
                }
                if (originalSize.Height < Size_Square)
                {
                    squareHeight = originalSize.Height;
                }
                return new Size(squareWidth, squareHeight);
			}

			if (largerInt > 0)
			{
				if (isWidthLarger)
				{
					size.Width = largerInt;
					size.Height = GetSmallerInt(originalSize, sizeType, isWidthLarger, largerInt);
				}
				else
				{
					size.Height = largerInt;
					size.Width = GetSmallerInt(originalSize, sizeType, isWidthLarger, largerInt);
				}
			}

			return size;
		}

		public static bool Equals(EImageSizeType type, string typeStr)
		{
			if (string.IsNullOrEmpty(typeStr)) return false;
			if (string.Equals(GetValue(type).ToLower(), typeStr.ToLower()))
			{
				return true;
			}
			return false;
		}

	}
}
