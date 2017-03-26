using System;

namespace BP_pokus_3
{																																				/// <summary> /// Description of Neuron. /// </summary>
	
	[Serializable]
	public class Neuron
	{
		public	double [] weights, input;
		public	double output, grad, sum ;
	    public	Neuron next;
			
			
		public Neuron(int vrstva)
		{	
			if (vrstva==0){
				weights = new double[Neuronet.inputLength];
				input = new double[Neuronet.inputLength];
			}
			if (vrstva==1){
				weights = new double[Neuronet.prvniVrstva];
				input=new double[Neuronet.prvniVrstva];
			}
			if (vrstva==2){
				weights = new double[Neuronet.druhaVrstva];
				input=new double[Neuronet.druhaVrstva];
			}
			doRandomWeights();
		}		
			
		
		public void doRandomWeights()							
		{  
			Random rand = new Random();
			for (int i=0;i<weights.Length; i++){
				do{
			 	weights[i] =rand.Next(-6,5)*0.1 + 0.1;
				} while (weights[i]==0);
			}
		}		
		
		public	double countOut()
		{
			sum=0;
			output=0;
			for (int i=0; i<input.Length; i++)
			{
				sum+=weights[i]*input[i];
			}
			output=1.7159*Math.Tanh(0.66*sum);
		    return output;
		}
		
	}
}

