using System;
using System.Collections.Generic;

namespace BP_pokus_3
{
	/// <summary>
	/// To discripe convoluce and list of inputs with their average value.
	/// </summary>
	public class Convolution
	{
		public double[,] weights;
		public AverageInput[,] avInput;					// pro pocitani stredni hodnoty vstupu kazde vahy 
		public int cisloFiltra;
		public int size;
		public double grad;
		public double averageOutput;
		public LinkedList<double> allOutputs = new LinkedList<double>();
		
		public struct AverageInput {
			public double average;
	     	public LinkedList<Double> inputList;
		}
		
		public Convolution(int size, int cisloFiltra)
		{
			weights = doRandomWeights(new double[size,size]);
			avInput = new AverageInput[size,size];
			this.size = size;
			for (int i=0; i<size; i++) {
				for (int j=0; j<size; j++) {
					avInput[i,j].inputList = new LinkedList<double>();
				}
			}
		}
		
		double[,] doRandomWeights(double[,] newFilter) {
			Random rand = new Random();
			for(int i=0; i<newFilter.GetUpperBound(0)+1; i++) {
				for(int j=0; j<newFilter.GetUpperBound(1)+1; j++) {
					do{
				 		newFilter[i,j] = rand.Next(-6,5)*0.1 + 0.1;
					} while (newFilter[i,j] == 0);
				}
			}
			return newFilter;
		}
		
		public void countAverageOutput() {
			LinkedListNode<double> templ = allOutputs.First;
			for(int i=0; i<allOutputs.Count; i++) {
				averageOutput += templ.Value;
				templ = templ.Next;
			}
			averageOutput = averageOutput/allOutputs.Count;
		}
		
		public void countAverageInput() {
			for (int i=0; i<size; i++) {
				for (int j=0; j<size; j++) {
					LinkedListNode<Double> templ = avInput[i,j].inputList.First;
					for(int k=0; k<avInput[i,j].inputList.Count; k++) {
						avInput[i,j].average += templ.Value;
						templ = templ.Next;
					}
					avInput[i,j].average = avInput[i,j].average/avInput[i,j].inputList.Count;
				}
			}
		}
	}
}
