using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesktopPet.Tools
{
    /// <summary>
    /// Convert the current animation XML to a DOT file.
    /// </summary>
    /// <remarks>
    /// A DOT file can be opened with Graphviz (or http://www.webgraphviz.com/) to generate a graphically view of the XML.
    /// </remarks>
	class XmlToDot
	{	
		static private string animationTitle;

        /// <summary>
        /// Convert the Xml to a DOT and returns the result as string.
        /// </summary>
        /// <param name="model">The root node of xml file.</param>
        /// <returns></returns>
		static public string ProcessXml(XmlData.RootNode model)
		{
			var animations = model.Animations;
			if (model.Animations == null)
			{
				Console.WriteLine("No animations found exiting.");
				return null;
			}
			animationTitle = model.Header.Title;
			return ProcessAnimations(model.Animations.Animation);
		}

		static private string ProcessAnimations(XmlData.AnimationNode[] animations)
		{
			Console.WriteLine($"# Processing {animations.Length} animations.");

			string returnString = "";
			returnString += $"# Convert {animationTitle} to Graphviz dot format by DesktopPet Xml2Gv {DateTime.Now}\r\n";
			returnString += $"# Copy the text and insert it into http://dreampuf.github.io/GraphvizOnline/ or http://webgraphviz.com/ to generate an image\r\n";
			returnString += $"# This functionality was added after this isse: https://github.com/Adrianotiger/desktopPet/issues/6 \r\n";
			returnString += $"digraph PetGraph {{\r\n";
			returnString += $" rankdir = LR;\r\n";

			foreach (var anim in animations)
			{
				if (anim.Sequence.Next != null)
				{
					returnString += $"# {anim.Id} Sequence {anim.Sequence.Next.Length}\r\n";
					var totalProbability = anim.Sequence.Next.Sum(n => n.Probability);
					returnString += ProcessNext(Next.Sequence, totalProbability, anim, anim.Sequence.Next);
				}

				if (anim.Border != null && anim.Border.Next != null)
				{
					returnString += $"# {anim.Id} Border {anim.Border.Next.Length}\r\n";
					var totalProbability = anim.Border.Next.Sum(n => n.Probability);
					returnString += ProcessNext(Next.Border, totalProbability, anim, anim.Border.Next);
				}

				if (anim.Gravity != null && anim.Gravity.Next != null)
				{
					returnString += $"# {anim.Id} Gravity {anim.Gravity.Next.Length}\r\n";
					var totalProbability = anim.Gravity.Next.Sum(n => n.Probability);
					returnString += ProcessNext(Next.Gravity, totalProbability, anim, anim.Gravity.Next);
				}

				returnString += $"  anim_{anim.Id} [ label=\"{anim.Name} ({anim.Id})\" ]\r\n";
			}
			returnString += $"}}\n";
			return returnString;
		}

		private enum Next { Sequence, Border, Gravity };

		private const string sequenceColor = "black";
		private const string borderColor = "#5555DDFF";
		private const string gravityColor = "#55DD55FF";

		static private string ProcessNext(Next type, int totalProbability, XmlData.AnimationNode anim, XmlData.NextNode[] nexts)
		{
			string returnString = "";
			var edgeColor = sequenceColor;
			var typeMarker = "S";
			if (type == Next.Border) { edgeColor = borderColor; typeMarker = "B"; }
			if (type == Next.Gravity) { edgeColor = gravityColor; typeMarker = "G"; }

			foreach (var next in nexts)
			{
				var relative = (double)next.Probability / totalProbability;
				edgeColor = type == Next.Sequence ? convertProbabilityToGray(relative) : edgeColor;
				var relative2Decimal = relative.ToString("00%");
				var probability = relative2Decimal == "100%" ? "" : $"({next.Probability})";
				var label = $"[ label=\"{relative2Decimal}{probability} {next.OnlyFlag} {typeMarker}\" color=\"{edgeColor}\" fontcolor=\"{edgeColor}\" penwidth=\"1\" ]";
				returnString += $"  anim_{anim.Id} -> anim_{next.Value} {label}\r\n";
			}
			return returnString;
		}

		static private string convertProbabilityToGray(double relativeProbability)
		{
			// convert prob of 0 to 1 to gray50 down to gray0(black)
			// linear mapping.
			//var p1 = Math.Floor((0.5 + (relativeProbability / 2)) * 100);
			//var p2 = 50 - (p1 - 50);

			var p1 = Math.Floor((0.3 + (relativeProbability / (1 / 0.7))) * 100);
			// range 30 to 100
			var p2 = 70 - (p1 - 30);

			// input range 50 to 100.
			// output range 50 to 0.
			var p3 = p2.ToString();
			//p2 = p2 == "100" ? "0" : p2;
			Console.WriteLine($"# {relativeProbability}, {p1}, {p2}, {p3}");
			return $"grey{p3}";
			//return "grey36";
		}
	}
}
