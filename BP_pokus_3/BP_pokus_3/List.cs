using System;

namespace BP_pokus_3
{
	/// <summary>
	/// List of neurons
	/// </summary>
	
	[Serializable]
	public class List
	{
		public	int length;
		public Neuron head;
	    public double [] outputs;
	    int vrstva;
		
		
		public List(int vrstva)
		{
			this.vrstva=vrstva;
			if (vrstva==0)
				length=Neuronet.prvniVrstva;
			if(vrstva==1)
				length=Neuronet.druhaVrstva;
			if(vrstva==2)
				length=Neuronet.tretiVrstva;
			Neuron templ;
			for (int i=0; i< length; i++){
				Neuron node=new Neuron(vrstva);
				if (i==0)
					head=node;
				else {
					 templ=head;
					 while(templ.next!=null){
					 	templ = templ.next;
					 }
					 
					 do{
					   node= new Neuron(vrstva);
					 } while(templ.weights[0]==node.weights[0]);
					 
					 templ.next=node;
					 }
			}
			outputs=new double[length];
		}
		
			
		public void writeInput(double[] input){
			Neuron templ=head;
			for (int i=0; i<Neuronet.prvniVrstva; i++){
				templ.input = new double[Neuronet.inputLength];
				for (int j=0; j<input.Length; j++){														
					templ.input[j]= input[j];
				}
				templ=templ.next;
			}
		}
		
		
		public void countOutputs(){
			Neuron templ=head;
			int counter=0;
			while(templ!=null){
				outputs[counter]=templ.countOut();
				templ= templ.next;
				counter++;
			}
		}
	}
}
