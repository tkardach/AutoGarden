using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content;
using Java.Lang;
using System.Linq;
using static Android.Graphics.Pdf.PdfDocument;

namespace AutoGarden.Droid.GardenGridView
{
	public class PlantPage : Page
	{
		public PlantPage(Plant plant, Type activity) : base(activity)
        {
            Plant = plant;
        }

		public Plant Plant { get; private set; }
	}

	public class Page
    {
        public Page(Type activity)
		{
			PageType = activity;
		}

        public Type PageType { get; private set; }
    }

	public class GardenGridAdapter : BaseAdapter<PlantPage>
    {
		List<PlantPage> m_plantItems;
		Context m_context;

		View m_plantAddButton;

		public GardenGridAdapter(Context context, IEnumerable<Plant> list)
		{
			m_context = context;
			m_plantItems = new List<PlantPage>();
			list.ToList().ForEach(o => m_plantItems.Add(new PlantPage(o, typeof(PlantViewActivity))));

            LayoutInflater inflater =
                (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
			m_plantAddButton = inflater.Inflate(Resource.Layout.GridAddItem, null);
			m_plantAddButton.SetOnClickListener(null);
		}

		public override PlantPage this[int position] => m_plantItems[position];

		public override int Count 
		{
			get { return m_plantItems.Count + 1; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}
        
		public View AddPlantView
		{
			get { return m_plantAddButton; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			LayoutInflater inflater = 
				(LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);

			View gridItem;
			if (convertView == null)
			{
				if (position == m_plantItems.Count)
				{
					return AddPlantView;
				}
				else
				{
                    gridItem = new View(m_context);
                    gridItem = inflater.Inflate(Resource.Layout.PlantGridItem, null);
				}
			}
			else
			{
				if (position == m_plantItems.Count)
					return AddPlantView;
                else
                {
					if (convertView.FindViewById(Resource.Layout.PlantGridItem) == null)
					{
						gridItem = new View(m_context);
						gridItem = inflater.Inflate(Resource.Layout.PlantGridItem, null);
					}
					else
						return AddPlantView;
                }
			}

            TextView plantName =
                (TextView)gridItem.FindViewById(Resource.Id.plantGridItemName);

            plantName.Text = m_plantItems[position].Plant.Name;

            ImageView plantImage =
                (ImageView)gridItem.FindViewById(Resource.Id.plantGridItemImage);

            plantImage.SetImageResource(Resource.Drawable.doge);


			return gridItem;
		}


	}
}
