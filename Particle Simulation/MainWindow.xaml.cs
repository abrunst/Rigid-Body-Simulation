﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Particle_Simulation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// The GeometryGroup for the Bodies
		/// </summary>
		private GeometryGroup bodyGroup = new GeometryGroup();

		/// <summary>
		/// The GeometryGroup for the movingBodies
		/// </summary>
		private GeometryGroup movingBodyGroup = new GeometryGroup();

		/// <summary>
		/// The path for the Bodies
		/// </summary>
		private Path bodyPath = new Path();

		/// <summary>
		/// The Path for the movingBodies
		/// </summary>
		private Path movingBodyPath = new Path();

		/// <summary>
		/// List of all the Bodies, not including movingBodies
		/// </summary>
		private List<Body> bodies = new List<Body>();

		/// <summary>
		/// List of all the movingBodies
		/// </summary>
		private List<MovingBody> movingBodies = new List<MovingBody>();

		/// <summary>
		/// List of all the Bodies and movingBodies
		/// </summary>
		private List<Body> allBodies = new List<Body>();

		/// <summary>
		/// The radius of the earth
		/// </summary>
		private double earthRadius = 6.38e+6;

		/// <summary>
		/// The mass of the earth
		/// </summary>
		private const double earthMass = 5.972e+24;

		/// <summary>
		/// The gravitational constant
		/// </summary>
		private const double gravitationalConstant = 6.674e-11;

		/// <summary>
		/// The Body that is being dragged, else null
		/// </summary>
		private Body dragging;

		/// <summary>
		/// The time of the last render
		/// </summary>
		private TimeSpan lastRender;

		/// <summary>
		/// The time until the next render
		/// </summary>
		private double timeUntilRender = 0;

		private VisualCollection visualCollection;

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			SetDefaults();
		}

		//Sets the default values for fields and properties
		public void SetDefaults()
		{
			bodyGroup.FillRule = FillRule.Nonzero;
			movingBodyGroup.FillRule = FillRule.Nonzero;

			drawingArea.Children.Add(bodyPath);
			drawingArea.Children.Add(movingBodyPath);

			bodyPath.Data = bodyGroup;
			bodyPath.Fill = new SolidColorBrush(Color.FromArgb(150,105,105,105));
			bodyPath.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));
			bodyPath.StrokeThickness = 4;
			bodyPath.Fill.Freeze();
			bodyPath.Stroke.Freeze();

			movingBodyPath.Data = movingBodyGroup;
			movingBodyPath.Fill = new SolidColorBrush(Color.FromArgb(150, 192, 192, 192));
			movingBodyPath.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));
			movingBodyPath.StrokeThickness = 4;
			movingBodyPath.Fill.Freeze();
			movingBodyPath.Stroke.Freeze();

			//Setting the size of the window to 80% of the screen
			this.Height = (System.Windows.SystemParameters.FullPrimaryScreenHeight * 0.80);
			this.Width = (System.Windows.SystemParameters.FullPrimaryScreenWidth * 0.80);

			///Setting lastRender
			lastRender = TimeSpan.FromTicks(DateTime.Now.Ticks);


			CompositionTarget.Rendering += CompositionTarget_Rendering;
			//AddParticle(50, new Point(250, 250), Color.FromArgb(255, 200, 73, 92));
		}

		/// <summary>
		/// Handler for the rendering event
		/// Updates the positions of the Bodies
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			//Casting to RenderingEventArgs to get the rendering time
			RenderingEventArgs renderArgs = (RenderingEventArgs)e;

			//Gets the time until next render
			timeUntilRender = (renderArgs.RenderingTime - lastRender).TotalSeconds;
			lastRender = renderArgs.RenderingTime;

			//Applies the force of each Body to each movingBody if it is not being dragged
			foreach(MovingBody movingBody in movingBodies)
			{
				if (movingBody != dragging)
				{
					foreach (Body body in bodies)
					{
						movingBody.VelocityVerlet(body, timeUntilRender);

					}
				}
			}

			//If a Body is being dragged, update its coordinates
			if (dragging != null)
			{
				dragging.Coordinates = Mouse.GetPosition(drawingArea);
			}
		}

		/// <summary>
		/// When the selected color is changed in the colorPicker, change the fill of the path
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			//Not yet implemented
		}

		/// <summary>
		/// On MouseLeftButtonDown in the drawingArea, decides what to do based on the selection in onClickComboBox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">The mouse event that triggered the handler</param>
		private void DrawingArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (onClickComboBox.SelectedIndex == 0)
			{
				ClickedInside(e.GetPosition(drawingArea));
			}
			else if (onClickComboBox.SelectedIndex == 1)
			{
				Console.WriteLine(e.GetPosition(drawingArea));
				float radius = (float)radiusIUD.Value;
				Point coordinates = e.GetPosition(drawingArea);

				AddBody(radius, coordinates);
			}
			else if (onClickComboBox.SelectedIndex == 2)
			{
				Console.WriteLine(e.GetPosition(drawingArea));
				float radius = (float)radiusIUD.Value;
				Point coordinates = e.GetPosition(drawingArea);

				AddMovingBody(radius, coordinates);
			}
			else if (onClickComboBox.SelectedIndex == 3)
			{
				foreach (Body body in allBodies)
				{
					if (dragging is null && body.IsInside(e.GetPosition(drawingArea)))
					{
						dragging = body;
						if (dragging is MovingBody)
						{
							((MovingBody)dragging).Velocity = new Vector();
						}
						
					}
				}
			}
		}

		/// <summary>
		/// Determines if a click is inside a Body
		/// </summary>
		/// <param name="x">The x coordinate of the click</param>
		/// <param name="y">The y coordinate of the click</param>
		public void ClickedInside(Point coordinates)
		{
			foreach (Body body in bodies)
			{
				if (body.IsInside(coordinates))
				{

				}
			}
		}

		/// <summary>
		/// Adds a Body to the geometryGroup
		/// </summary>
		/// <param name="radius">The radius of the particle</param>
		/// <param name="coordinates">The cooridnates of the particle</param>
		/// <param name="color">The color of the particle</param>
		public void AddBody(float radius, Point coordinates)
		{
			Body body = new Body(coordinates, radius);
			bodies.Add(body);
			allBodies.Add(body);

			body.AddToDrawing(bodyGroup);
		}

		/// <summary>
		/// Adds a MovingBody to the GeometryGroup
		/// </summary>
		/// <param name="radius">The radius of the particle</param>
		/// <param name="coordinates">The cooridnates of the particle</param>
		public void AddMovingBody(float radius, Point coordinates)
		{
			MovingBody movingBody = new MovingBody(coordinates, radius);
			movingBodies.Add(movingBody);
			allBodies.Add(movingBody);

			movingBody.AddToDrawing(movingBodyGroup);
		}

		/// <summary>
		/// Applies gravity to the MovingBody
		/// </summary>
		/// <param name="particle"></param>
		private void Gravity(MovingBody particle)
		{
			double height = drawingArea.ActualHeight - particle.Coordinates.Y - particle.Radius + earthRadius;
			particle.Force = Vector.Add(particle.Force, new Vector(0,  ((gravitationalConstant * earthMass) / (height * height))));
			particle.Acceleration = Vector.Divide(particle.Force, particle.Mass);
			particle.Velocity = Vector.Add(particle.Velocity, Vector.Multiply(particle.Acceleration, timeUntilRender));
		}

		/// <summary>
		/// Handler for the left mouse button being released
		/// Sets draggin to null if it is not already
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DrawingArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (dragging != null)
			{
				dragging = null;
			}
		}

		/// <summary>
		/// Handler for a click on the clearButton
		/// On click, clears the Bodies in bodyGroup and the movingBodies in movingBodyGroup
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			bodyGroup.Children.Clear();
			movingBodyGroup.Children.Clear();
			bodies = new List<Body>();
			movingBodies = new List<MovingBody>();
		}
	}
}
